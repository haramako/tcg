# coding: utf-8
# frozen_string_literal: true

#
# ファイル名の命名規則チェッカー

require 'active_support/inflector'

ignore_list = Regexp.union(
  [
    'Doc/', # TODO: そのうち直す

    # 他人のコードは気にしない
    '/ToydeaLib/NintendoSDKPlugin/',
    '/Vendor/',

    'Client/AssetBundles', # アセットバンドルは小文字
    'Client/Assets/Slua',
    'Client/.vscode/',
    'Data/Tmx/', # マップデータは Room6x6_001.tmx のような名前なので例外
    'Data/Master/Message_',

    'Doc/Common/FangIcon/',
    'Experiment/',
    'webui_assets/',
    'memo.md',

    'index.md',
    'Tools/Bin/',
    'Tools/CodeGen/',
    'Tools/Converter/',
    'Tools/Misc/',
    'Tools/Protocols/',
    'Tools/RubyLib/',
    'Tools/UnityCloudDownloader/',
    'Tools/WebUi/',
    'Doc/_',
    'Doc/assets',
    'Doc/jekyll',
    'Doc/Tools/Execution/',
    'UnityPackageManager/manifest.json',
    '/Xlsx/', # Docのimagesはよい
    '.bucket',
    '.files/', # XLSのHTML
    'ca-bundle.crt',
    'index.html',
    %r{/Edit_.*\.unity$},
    /\.otf$/,
    /\.ttf$/,
    /\.rake$/,
    /\.rb$/,
    %r{Doc/.*/Images/}i
  ]
)

files = `git ls-files`.gsub(/"/) { '' }
files.split(/\n/).each do |f|
  basename = File.basename(f)
  extname = File.extname(basename)
  next if f.start_with? '.'
  next if extname =~ /\.(meta|ab|manifest)/
  name = File.basename(basename, extname)
  next if name.camelize == name
  next if f =~ ignore_list
  puts "命名規則違反 %-30s at %s" % [basename, f]
end
