# -*- coding: utf-8 -*-
# frozen_string_literal: true

require_relative 'short_json.tab.rb'
require 'strscan'

module ShortJson
  class ParseError < RuntimeError
  end

  class Parser
    def initialize(src)
      @scanner = StringScanner.new(src)
      @paren_level = 0
      @after_comma = false
    end

    def next_token
      # コメントと空白を飛ばす
      while @scanner.scan(%r{\s+ | //.+?\n | /\*.+?\*/ }mx)
        if @paren_level == 0 && !@after_comma && @scanner[0].include?("\n")
          return [',', ',']
        end
        @after_comma = false
      end

      if @scanner.eos?
        r = nil
      elsif @scanner.scan(/\(|\)|:|,|\[|\]/)
        # 記号
        sym = @scanner[0]
        case sym
        when '('
          @paren_level += 1
        when ')'
          @paren_level -= 1
        when ','
          @after_comma = true if @paren_level == 0
        end
        r = [sym, sym]
      elsif @scanner.scan(/-?\d+\.\d+/)
        # 浮動小数
        r = [:NUMBER, @scanner[0].to_f]
      elsif @scanner.scan(/-?\d+/)
        # 10進数
        r = [:NUMBER, @scanner[0].to_i]
      elsif @scanner.scan(/\w+/)
        # 識別子
        r = [:STRING, @scanner[0]]
      elsif @scanner.scan(/"([^\\"]|\\.)*"/)
        # ""文字列
        str = @scanner[0][1..-2]
        str = str.gsub(/\\n|\\x../) do |s|
          case s
          when '\n'
            "\n"
          when /^\\x/
            n = s[2..-1]
            if n !~ /[0-9a-fA-F]+/
              raise ShortJson::ParseError, "invalid character '#{s}' in string"
            end
            n.to_i(16).chr.force_encoding(Encoding::UTF_8)
          end
        end
        r = [:STRING, str]
      elsif @scanner.scan(/'([^\\']|\\.)*'/)
        # ''文字列
        r = [:STRING, @scanner[0][1..-2]]
      else
        # :nocov:
        raise "invalid token at #{@scanner.pos}"
        # :nocov:
      end
      r
    end

    def make_args(vals)
      obj = {}
      i = 0
      vals.each do |k, v|
        if k
          obj[k.to_sym] = v
        else
          obj[i] = v
          i += 1
        end
      end
      obj
    end

    def parse(is_array = false)
      ast = do_parse
      if is_array
        ast
      else
        raise "must be one object" if ast.size != 1
        ast[0]
      end
    rescue Racc::ParseError => err
      raise "Error #{err} at #{@scanner.pos}"
    end
  end

  module_function

  # ShortJson.parse('hoge(fuga:1,piyo:2)') # => {"type":"hoge", "fuga":1, "piyo":2}
  def parse(src, is_array = false)
    Parser.new(src).parse(is_array)
  end

  def parse_array(str)
    str.split(/,/).map { |element| parse(element) }
  end
end
