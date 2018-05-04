# coding: utf-8
# frozen_string_literal: true

module PbReporting
  def parse_pb(bin)
    if bin[0, 20] =~ /PBX/
      unpack_pbx(bin)
    else
      unpack_pb_list(bin)
    end
  rescue
    puts $ERROR_INFO
    []
  end

  # pbのデータの概要をレポートする
  def report_chunk_lists(files)
    puts '-' * 80
    puts('%-36s %-20s %4s %8s %8s        %6s' % ['filename', 'sheet', 'ext', 'size', 'gz-size', 'items'])
    puts '-' * 80
    total_size = 0
    total_gz_size = 0
    files.each do |f|
      data = IO.binread(f)
      gz_size = Zlib::Deflate.deflate(data).size
      size = data.size
      comp = 100 * gz_size / size
      begin
        _, list = PbConvert.unpack_chunk_list(data)
      rescue
        list = []
      end
      fname = File.basename(f).gsub(/.pbx?$/, '').split(/-/)
      puts('%-36s %-20s %-4s %8d %8d (%3d%%) %6d' % [fname[0], fname[1], File.extname(f), size, gz_size, comp, list.size])
      total_size += size
      total_gz_size += gz_size
    end
    puts '-' * 80
    puts "トータルサイズ    : #{total_size}"
    puts "圧縮後サイズ      : #{total_gz_size}"
    puts "圧縮率            : #{100 * total_gz_size / total_size}%"
  end
end
