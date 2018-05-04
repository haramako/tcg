# frozen_string_literal: true

module PbIndexList
  def pack(dict)
    type_name = (dict.values[0].class.descriptor.name.encode('ASCII') rescue 'String')
    pos = 0
    index = Master::PbxHeader.new

    list_bin = dict.map do |kv|
      if kv[0].is_a? Numeric
        index.int_index[kv[0]] = pos
      else
        index.string_index[kv[0].to_s] = pos
      end
      bin = Zlib::Deflate.deflate(kv[1].class.encode(kv[1]))
      pos += 4 + bin.size
      bin
    end

    index_bin = Master::PbxHeader.encode(index)
    pack_chunk_list("PBX/#{type_name}", [index_bin] + list_bin)
  end

  def unpack(bin)
    header, list = unpack_chunk_list(bin)
    mo = header.match(%r{^PBX/(.*)$})
    raise "invalid pbx header '#{header}'" unless mo

    # rubocop:disable Security/Eval
    type = eval(mo[1].gsub(/\./) { '::' })
    # rubocop:enable Security/Eval

    index_bin = list.shift
    index = Master::PbxHeader.decode(index_bin)
    header_index_size = 1 + 1 + header.size + 4 + index_bin.size

    r = {}
    (index.int_index.to_a + index.string_index.to_a).each do |kv|
      pos = header_index_size + kv[1]
      len = bin[pos, 4].unpack('L')[0]
      obj = type.decode(Zlib::Inflate.inflate(bin[pos + 4, len]))
      r[kv[0]] = obj
    end
    r
  end
end
