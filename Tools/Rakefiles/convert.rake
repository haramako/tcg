# coding: utf-8
# frozen_string_literal: true

# ほげ

require 'active_support/inflector'
require 'pathname'
require 'yaml'
require 'open-uri'
require 'stringio'

require 'xls_to_pb_converter'
require 'tmx'
begin
  require 'autogen/master_pb'
  require 'augogen/game_pb'
rescue LoadError
  nil
end

#===================================================
# 定数
#===================================================

LUA_DIR = DATA_DIR + 'Lua'
OTHER_DIR = DATA_DIR + 'Other'
PROTO_DIR = PROJECT_ROOT + 'Tools/Protocols'

mkdir_p [OUTPUT, TEMP], verbose: false

#===================================================
# 便利関数
#===================================================

# String#pathmap にもとづいて、タスクを作成して、出力ファイルのリストを取得する
#
# 使用例:
# OUT = map_task(['a.txt', 'b.txt'], 'out/%n.bin') do |t|
#   puts t.source
# end
# OUT # => ['out/a.bin', 'out/b.bin']
def pathmap_task(filelist, pathmap_pattern)
  raise "pathmap_pattern must be String but #{pathmap_pattern.class}" unless pathmap_pattern.is_a? String
  filelist.map do |src|
    out = src.pathmap(pathmap_pattern)
    file out => src do |t|
      yield t
    end
    out
  end
end

# rubocop:disable Style/GlobalVars
def logger
  unless $_logger
    $_logger = Logger.new(STDOUT)
    $_logger.level = Logger::INFO
  end
  $_logger
end
# rubocop:enable Style/GlobalVars

# Inflectorに単語登録
ActiveSupport::Inflector.inflections do |inf|
  inf.singular 'bonus', 'bonus'
  inf.singular 'data', 'data'
end

#===================================================
# タスクリスト
#===================================================

task :default => [:map, :master, :lua, :other, :copy]

task :clean do
  rm_rf [OUTPUT, TEMP, DATA_DIR + 'OutputPack']
end

OUT_MAPS = pathmap_task(FileList[DATA_DIR.to_s + '/**/*.tmx'].exclude('**/MetaMap*.tmx'), OUTPUT.to_s + '/%n_Stage.pb') do |t|
  logger.info "TMXコンバート中 #{t.source}"
  map = Tmx.new(t.source)
  IO.binwrite(t.name, map.dump_pb)
end

OUT_META_MAPS = pathmap_task(FileList[DATA_DIR.to_s + '/Tmx/MetaMap/*.tmx'], OUTPUT.to_s + '/%n_MetaMapTmx.pb') do |t|
  logger.info "MetaMapコンバート中 #{t.source}"
  map = Tmx.new(t.source)
  IO.binwrite(t.name, map.dump_meta_map_pb)
end

desc 'マップファイルの変換'
task :map => OUT_MAPS + OUT_META_MAPS

task :xls do
  sh 'ruby', 'Tools/Converter/converter', '-o', OUTPUT.to_s, (DATA_DIR + 'Master').to_s
end

OUT_YAMLS = pathmap_task(FileList[DATA_DIR + 'Debug/**/*.yml'], OUTPUT.to_s + '/FromYaml_%n.pb') do |t|
  logger.info "Converting #{t.source}"

  model_class = Master.const_get(File.basename(t.source, '.yml'))
  next unless model_class

  data = YAML.parse(IO.read(t.source)).to_ruby
  data = data.map { |row| PbConvert.conv_message(model_class, row) }

  IO.binwrite(t.name, PbConvert.pack_pb_list(data))
end

desc 'マスターファイルの変換'
task :master => [:xls, *OUT_YAMLS]

desc '.pbファイルのYAML化'
task :to_yaml do
  require 'yaml'
  mkdir_p TEMP + 'ChunkDump'
  FileList[OUTPUT.to_s + '/*.pb'].each do |f|
    logger.info "YAMLへコンバート中 #{File.basename(f)}"
    yml = PbConvert.parse_pb(IO.binread(f)).map do |item|
      if item.is_a? Array
        if item[1].is_a? String
          data = item
        else
          data = JSON.parse(item[1].to_json)
        end
      else
        data = JSON.parse(item.to_json)
      end
      YAML.dump(data)
    end.join("\n")
    IO.binwrite(TEMP + 'ChunkDump' + "#{File.basename(f)}.yml", yml)
  end
end

desc '.pbファイルのレポート'
task :report do
  files = []
  FileList[OUTPUT.to_s + '/**/*'].each do |f|
    next unless f =~ /\.pb(x)?$/
    files << f
  end
  PbConvert.report_chunk_lists(files)
end
