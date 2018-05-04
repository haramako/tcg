# coding: utf-8
# frozen_string_literal: true

require 'rexml/document'
require 'pp'
require 'ostruct'

# モンキーパッチ
class REXML::Element
  def a
    attributes
  end
end

class Tmx
  attr_reader :filename, :width, :height, :layers, :tiles

  def initialize(tmx)
    @filename = tmx
    @doc = REXML::Document.new(IO.read(tmx))
    map = @doc.elements['map']
    @width = map.a['width'].to_i
    @height = map.a['height'].to_i

    @layers = []
    map.each_element('layer') do |l|
      @layers << l.elements['data'].text.split(/,/).map(&:to_i).each_slice(@width).to_a.reverse
    end

    @objects = []
    map.each_element('objectgroup') do |group|
      group.each_element('object') do |obj|
        x = obj.a['x'].to_i / 64
        y = @height - obj.a['y'].to_i / 64
        name = obj.a['name']
        props = obj.elements['properties']
        next unless props
        obj = OpenStruct.new(x: x, y: y)
        props.each_element('property') do |prop|
          name = prop.a['name']
          val = prop.a['value']
          case name
          when 'type'
            obj.__send__(name + '=', val)
          when 'id', 'rest_count', 'dungeon', 'floor', 'monster_num1', 'monster_num2', 'monster_num3',
            'fence_prob', 'item_num1', 'item_num2', 'dir', 'level', 'hint_id'
            obj.__send__(name + '=', val.to_i)
          when 'keep_item', 'cursed', 'unreveal', 'keep_all'
            obj.__send__(name + '=', val == 'true')
          when 'attribute'
            obj.attribute = val.split(/,/).map { |str| str.strip.classify.to_sym }
          when 'fence', 'script'
            # DO NOTHING つかっていないプロパティ
            _ = nil
          else
            raise "unknown field #{name}"
          end
        end
        @objects << obj
      end
    end

    make_tile
    make_events
  end

  def make_tile
    @tiles = Array.new(@width * @height) { 0 }
    @layers.each do |layer|
      (0...@height).each do |y| # TODO: -1 は不要なはず、調査して消す
        (0...@width).each do |x| # TODO: -1 は不要なはず、調査して消す
          i = y * @width + x
          cell = layer[y][x]
          case cell
          when 0..24
            @tiles[i] |= cell
          when 25..129
            @tiles[i] |= (cell - 24) << 8
          else
            raise "invalid tile #{l[i]} at #{i}"
          end
        end
      end
    end
    @tiles
  end

  def make_meta_map_tile
    @tiles = Array.new(@width * @height) { 0 }
    @layers.each do |layer|
      (0...@height).each do |y|
        (0...@width).each do |x|
          i = y * @width + x
          cell = layer[y][x]
          @tiles[i] = cell
        end
      end
    end
    @tiles
  end

  def make_events
    # DO NOTHING
  end

  def dump_pb
    events = []
    characters = []
    items = []
    rooms = []
    @objects.each do |obj|
      begin
        case obj.type
        when 'enemy'
          hash = obj.to_h
          hash.delete(:type)
          characters << Master::StageCharacter.new(hash)
        when 'item'
          hash = obj.to_h
          hash.delete(:type)
          items << Master::StageItem.new(hash)
        when 'player', 'stair', 'script', 'mark', 'arrange'
          obj.type = obj.type.classify.to_sym
          events << Master::StageEvent.new(obj.to_h)
        when 'room'
          hash = obj.to_h
          hash.delete(:type)
          rooms << Master::StageRoom.new(hash)
        else
          raise "オブジェクトのtype #{obj.type} は不正です。位置=(#{obj.x},#{obj.y})"
        end
      rescue
        raise "不正なオブジェクトです, 位置=(#{obj.x},#{obj.y}), オブジェクト=#{obj.to_h}"
      end
    end
    require 'digest/md5'
    id = Digest::MD5.hexdigest(@filename)[0, 6].to_i(16)
    pb = Master::Stage.new(
      id: id,
      symbol: File.basename(@filename, '.tmx'),
      width: @width,
      height: @height,
      tiles: @tiles.flatten,
      characters: characters,
      events: events,
      items: items,
      rooms: rooms
    )
    PbConvert.pack_pb_list([pb])
  end

  def dump_meta_map_pb
    make_meta_map_tile
    require 'digest/md5'
    id = Digest::MD5.hexdigest(@filename)[0, 6].to_i(16)
    pb = Master::MetaMapTmx.new(
      id: id,
      symbol: File.basename(@filename, '.tmx'),
      width: @width,
      height: @height,
      tiles: @tiles.flatten
    )
    PbConvert.pack_pb_list([pb])
  end
end
