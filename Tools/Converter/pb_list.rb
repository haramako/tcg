# coding: utf-8
# frozen_string_literal: true

module PbList
  module_function

  def pack(list)
    type_name = list[0].class.descriptor.name.encode('ASCII')
    pack_chunk_list(type_name, list.map { |x| x.class.encode(x) })
  end

  def unpack(_bin)
    type_name, list = unpack_chunk_list(s)
    # rubocop:disable Security/Eval
    type = eval(type_name.gsub(/\./) { '::' })
    # rubocop:enable Security/Eval
    list.map { |bin| type.decode(bin) }
  end

  # チャンクリストをパックする
  def pack_chunk_list(header, list)
    s = StringIO.new

    s.write ['C', header.size, header].pack('aCa*')

    list.each do |x|
      s.write [x.size, x].pack('La*')
    end

    s.string
  end

  # チャンクリストをアンパックする
  def unpack_chunk_list(s)
    s = StringIO.new(s) if s.is_a? String

    magic, header_size = s.read(2).unpack('aC')
    raise "not a chunk list" if magic != 'C'
    header = s.read(header_size)

    list = []
    until s.eof?
      size = s.read(4).unpack('L')[0]
      list << s.read(size)
    end

    [header, list]
  end
end
