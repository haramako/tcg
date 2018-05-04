# frozen_string_literal: true

require 'spec_helper'
require 'short_json'

describe ShortJson do
  it 'symple literal' do
    expect(ShortJson.parse('x')).to eq("x")
  end

  it 'empty object' do
    expect(ShortJson.parse('x()')).to eq(0 => 'x')
  end

  it 'object with key-value list' do
    expect(ShortJson.parse('x(a:1, b:2)')).to eq(0 => "x", a: 1, b: 2)
  end

  it 'object with value list' do
    expect(ShortJson.parse('x(a, b)')).to eq(0 => "x", 1 => "a", 2 => "b")
  end

  it 'object with value list and key-value list' do
    data = { 0 => "x", 1 => "a", 2 => "b", c: 1, d: 2 }
    expect(ShortJson.parse('x(a, b, c:1, d:2)')).to eq(data)
  end

  it 'combined object' do
    expect(ShortJson.parse('x(1, y(2), 3)')).to eq(0 => "x", 1 => 1, 2 => { 0 => "y", 1 => 2 }, 3 => 3)
  end

  it 'new line as comma between objects' do
    expect(ShortJson.parse("x(1),x(2)", true)).to eq(ShortJson.parse("x(1)\nx(2)", true))
  end

  it 'new line as comma between objects and not after comma' do
    expect(ShortJson.parse("x(1),x(2)", true)).to eq(ShortJson.parse("x(1),\nx(2)", true))
  end

  it 'dont new line as comma not between objects' do
    expect { ShortJson.parse("x(1\n2)") }.to raise_error(RuntimeError)
  end

  it 'empty array' do
    expect(ShortJson.parse('[]')).to eq([])
  end

  it 'array 1' do
    expect(ShortJson.parse('[1]')).to eq([1])
  end

  it 'array 2' do
    expect(ShortJson.parse('[1,2]')).to eq([1, 2])
  end

  it 'array in object' do
    expect(ShortJson.parse('a([])')).to eq(0 => 'a', 1 => [])
  end

  it 'double-quated string' do
    expect(ShortJson.parse('"hoge"')).to eq('hoge')
  end

  it 'double-quated string' do
    expect(ShortJson.parse('"ho\nge"')).to eq("ho\nge")
  end

  it 'double-quated string' do
    expect(ShortJson.parse('"ho\\x99ge"')).to eq("ho\x99ge")
  end

  it 'double-quated string' do
    expect { ShortJson.parse('"ho\\xMMge"') }.to raise_error(ShortJson::ParseError)
  end

  it 'single-quated string' do
    expect(ShortJson.parse("'hoge'")).to eq('hoge')
  end
end
