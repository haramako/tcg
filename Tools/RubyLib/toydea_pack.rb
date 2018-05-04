# coding: utf-8
# frozen_string_literal: true

module ToydeaPack
  module_function

  def pack(files)
    data = files.map do |f|
      { filename: File.basename(f), data: IO.binread(f), pos: 0, len: 0 }
    end
    header = make_header(data)
    pos = header.size

    bin_list = [nil]
    data.each do |f|
      f[:pos] = pos
      f[:len] = f[:data].size
      pos += f[:len]
      bin_list << f[:data]
    end

    header2 = make_header(data)
    raise "Invalid header size #{header.bytesize} and #{header2.bytesize}" if header.bytesize != header2.bytesize # 前後でサイズが違ったらおかしいよね！
    bin_list[0] = header2
    bin_list.join
  end

  def unpack(bin)
    s = StringIO.new(bin)
    magick = s.read(2)
    raise "invalid magick, not 'TP'" if magick != "TP"
    num = s.read(4).unpack('L')[0]
    files = []
    num.times do
      name_len = s.read(1).unpack('C')[0]
      name = s.read(name_len).unpack('a*')[0]
      pos, len = s.read(8).unpack("LL")
      files << { filename: name, pos: pos, len: len }
    end
    files
  end

  def make_header(data)
    s = StringIO.new
    s.write "TP"
    s.write [data.size].pack("L")
    data.each do |f|
      s.write [f[:filename].bytesize, f[:filename], f[:pos], f[:len]].pack("Ca*LL")
    end
    s.string.force_encoding(Encoding::ASCII_8BIT)
  end
end
