# -*- coding: utf-8 -*-
# frozen_string_literal: true

# ファングアイコンの画像を合成する
# ImageMagick がインストールされている必要がある

require 'fileutils'
require 'pp'
require 'find'

# $LOAD_PATH << __dir__
# require 'master'
# require 'pb_convert'

module FangIconGenerator
  include FileUtils
  extend FileUtils

  module_function

  ORIG_DIR = 'OriginalImages/FangIcons'
  PARTS_DIR = ORIG_DIR + '/Parts'
  DEST_DIR = 'Client/Assets/Gardens/FangIcons'

  def sys(cmd)
    puts cmd if ENV['DEBUG']
    system cmd
  end

  def dest_icon(fang)
    dir = DEST_DIR + "/FangIcon%02d" % [fang.pack]
    mkdir_p dir
    "%s/FangIcon%04d.png" % [dir, fang.id]
  end

  def generate_icon(fang)
    return if File.exist?(dest_icon(fang))
    unless File.exist? fang.path
      puts "Skip #{fang.id}, because #{pang.path} not found"
      return
    end
    puts "generate fang #{fang.id}"
    # sys "magick convert #{back_icon(fang)} #{fang.path} -composite -compose src-over #{dest_icon(fang)}"
    # sys "magick convert #{back_icon(fang)} #{fang.path} -composite -compose src-over #{rarity_icon(fang)} -composite -compose src-over #{dest_icon(fang)}"
    # sys "magick convert #{fang.path} -composite #{back_icon(fang)} -composite -channel A -compose atop #{back_icon(fang)} #{dest_icon(fang)}"
    mkdir_p "Temp/FangIcon"
    tmp1 = "Temp/FangIcon/tmp#{fang.id}_1.png"
    tmp2 = "Temp/FangIcon/tmp#{fang.id}_2.png"
    mask = PARTS_DIR + "/Mask.png"
    frame = PARTS_DIR + "/Frame.png"
    sys "magick convert -resize 100x100 #{fang.path} #{tmp1}"
    sys "magick convert -composite -compose dst-atop #{tmp1} #{mask} #{tmp2}"
    sys "magick convert -composite #{tmp2} #{frame} #{dest_icon(fang)}"
  end

  def generate_icons
    fangs = FileList[ORIG_DIR + '/FangIcon*/FangIcon*.png'].map do |n|
      m = n.match(%r{/FangIcon(\d+)/FangIcon(\d+)})
      if m
        pack = m[1]
        id = m[2].to_i
        Fang.new(id, n, pack, 6)
      else
        nil
      end
    end.reject(&:nil?)

    fangs.each do |f|
      generate_icon(f)
    end

    # list = FileList[OUTPUT.to_s + '/**/*_ItemTemplate.pb'].to_a.flat_map do |f|
    #  PbConvert.parse_pb(IO.binread(f))
    # end

    # list = list.select{|i| i.type == :Fang }
  end

  Fang = Struct.new(:id, :path, :pack, :rarity)

  def run
    mkdir_p 'Temp/FangIcons'
    mkdir_p 'Client/Assets/Gardens/FangIcons'
    generate_icons
    # generate_icon(Dfang.master.fangs[16058])
  end
end
