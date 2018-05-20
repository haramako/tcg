# coding: utf-8
# frozen_string_literal: true

require 'pathname'
require 'fileutils'
require 'spreadsheet'
require 'optparse'
require 'excel'
require 'pb_converter'
require 'pb_list'
require 'short_json'

class XlsToPbConverter
  def initialize(opt = {})
    @namespace = opt[:namespace]
    @logger = opt[:logger] || default_logger
    @output = Pathname.new(opt[:output])
    FileUtils.mkdir_p @output
  end

  def convert_directory(dir)
    files = Dir.glob(Pathname.new(dir) + '**' + '*.xls')
    files.each do |f|
      convert_xls(f)
    end
  end

  def convert_xls(src)
    book = Spreadsheet.open(src)
    touchfile = @output + (File.basename(src, '.xls') + '.touch')

    return unless changed?(src, touchfile) # 時刻を比較して、touchファイルの方が新しいならなにもしない

    @logger.info "コンバート中 #{src}"

    book.worksheets.each do |sheet|
      model_name, file_prefix = split_sheet_name(sheet.name)

      pb_name = (File.basename(src, '.xls') + file_prefix + '_' + model_name + '.pb')

      model_class = (@namespace.const_get(model_name) rescue nil)
      unless model_class
        @logger.debug "Unknown model #{model_name}"
        next
      end

      # マスターデータを.pbファイルにコンバートする
      @logger.debug "Converting Sheet:#{sheet.name}, Model:#{model_name}, Output:#{pb_name}"
      items = conv_sheet(src, book, sheet.name, model_class)
      unless items.empty?
        bin = PbList.pack(items)
        IO.binwrite(@output + pb_name, bin)
      end
    end
    IO.write(touchfile, '')
  end

  private

  # シート名からモデル名とファイル名のプレフィックスの対に変換する
  #
  # ':' で分離して、２番目の方をモデル名として使う
  # 'Hoge' => ['', 'Hoge']
  # 'Hoge:Fuga' => ['Hoge', 'Fuga']
  def split_sheet_name(sheet_name)
    sheet_name_pair = sheet_name.split('.', 2)
    if sheet_name_pair.size == 2
      file_prefix = '_' + sheet_name_pair[0]
      model_name = sheet_name_pair[1]
    else
      file_prefix = ''
      model_name = sheet_name_pair[0]
    end
    [model_name, file_prefix]
  end

  def conv_sheet(path, book, sheet, item_type)
    conv = PbConverter.new(@logger)
    data = Excel.read_from_file(path, book, sheet)
    data = data.reject { |row| row[:unused] } # unusedが指定された行は使用しない
    data.each { |row| row.delete(:unused) } # unusedが指定された行は使用しない
    items = data.map do |row|
      conv.conv_message(item_type, row)
    end
    items
  end

  def default_logger
    logger = Logger.new(STDOUT)
    logger.level = :info
    logger
  end

  # ファイルが変更されているかを取得する
  def changed?(file, touchfile)
    return true unless File.exist?(touchfile)

    touchfile_mtime = File.mtime(touchfile)
    src_mtime = File.mtime(file)
    src_mtime > touchfile_mtime
  end
end
