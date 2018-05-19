# coding: utf-8
# frozen_string_literal: true

require 'google/protobuf'
require 'stringio'
require 'active_support/inflector'
require 'zlib'
Dir[File.expand_path("../model/", __FILE__) << '/*.rb'].each { |file| require file }

class PbConverter
  def initialize(logger)
    @field_of_index_cache = {}
    @logger = logger || Logger.new(nil)
  end

  # FieldDescripterがMapのエントリーかを判定する
  # fd: FieldDescripter
  def map_entry?(fd)
    fd.label == :repeated && fd.type == :message && fd.subtype.name.include?('_MapEntry_')
  end

  def field_of_index(class_, index)
    list = @field_of_index_cache[class_]
    unless list
      list = []
      class_.descriptor.each do |desc|
        list[desc.number] = desc
      end
      @field_of_index_cache[class_] = list
    end
    list[index]
  end

  def conv_message(class_, data)
    m = class_.new

    conv_message_hook(class_, data)

    data.each do |k, v|
      begin
        k, v = conv_field_name(class_, k, v)
        k = k.to_s

        # 数字のインデックスが来た場合は、名前に変換する
        if k =~ /^\d+$/
          desc = field_of_index(class_, k.to_i + 1)
          unless desc
            @logger.warn "#{class_.name} に #{k} が存在しません"
            break
          end
          k = desc.name
        end

        desc = class_.descriptor.lookup(k)
        if desc
          conv_field(m, desc, k, v)
        else
          @logger.warn "#{class_.name} に #{k} が存在しません"
        end
      rescue
        @logger.error "コンバートできません #{class_}.#{k} = #{v}"
        @logger.error data
        @logger.error $!.to_s
        # raise
      end
    end
    m
  end

  def conv_field(m, desc, k, v)
    if map_entry?(desc)
      f = m.__send__(k)
      v.each do |k2, v2|
        k2 = conv_type(desc.subtype.lookup('key'), k2)
        v2 = conv_type(desc.subtype.lookup('value'), v2)
        f[k2] = v2
      end
    elsif desc.label == :repeated
      f = m.__send__(k)
      v = [v] unless v.is_a? Array # 対象が配列なら 1 は [1] に変換する
      v.each { |v2| f << conv_type(desc, v2) }
    else
      begin
        v2 = conv_type(desc, v)
        desc.set m, v2
      rescue RangeError
        @logger.error "#{v} は #{desc.name} の値として不正です。"
        raise
      end
    end
  end

  def conv_type(desc, v)
    case desc.type
    when :enum
      if v.is_a? Array
        v.reduce(0) { |m, x| (m | desc.subtype.lookup_name(x.camelize.to_sym)) }
      else
        v.camelize.to_sym
      end
    when :bool
      if v.is_a? String
        !(v =~ /true/i).nil?
      else
        v
      end
    when :string
      v = v.to_s if v.is_a? Symbol
      # TODO: タグの置き換えを行う
      # v.gsub!(/<(\w+)?>/) do |x|
      #  tag = x[1..-2]
      #  if tag
      #    # TAGS[tag.downcase] || x
      #    x
      #  else
      #    x
      #  end
      # end
      v
    when :message
      conv_message(desc.subtype.msgclass, v)
    else
      v
    end
  end

  def conv_field_name(_class, k, v)
    case k
    when :element
      v = 'no_element' if v == ''
    end
    [k, v]
  end

  def conv_message_hook(_class, data)
    # TODO: hookをいれる
    data
  end
end
# rubocop:enable Metrics/ModuleLength
