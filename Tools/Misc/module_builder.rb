# coding: utf-8
# frozen_string_literal: true

# C#のモジュールのコンパイルを行い、各モジュールが分離されているかを確認する
# 分離を確認するだけで、DLLを作成すること自体は目的ではない

$LOAD_PATH << __dir__ + '/../RubyLib'

require 'rake'
require 'open3'
require 'path_detector'

PROJECT_ROOT = '.'

LIBS = [
  "#{UNITY}/Managed/UnityEngine.dll",
  "#{UNITY}/Managed/UnityEditor.dll",
  "#{UNITY}/UnityExtensions/Unity/GUISystem/UnityEngine.UI.dll",
  "#{PROJECT_ROOT}/Client/Assets/Vendor/Ionic.Zlib.dll",
  "#{PROJECT_ROOT}/Client/Assets/Vendor/RSG.Promise.dll",
  "#{PROJECT_ROOT}/Client/Assets/Standard Assets/ToydeaLib/GoogleProtocolBuffer/GoogleProtocolBuffer.dll",
  "#{PROJECT_ROOT}/Client/Assets/Standard Assets/Vendor/DOTween/DOTween.dll"
]

DEFINES = %w[
  UNITY5 UNITY_EDITOR
]

# '/' を パスセパレータに変換する
# Windowsだと変換しないと一部のコマンドがファイルを認識してくれないため
def slash(str)
  if File::ALT_SEPARATOR
    if str.is_a? Array
      str.map { |s| s.gsub('/', File::ALT_SEPARATOR) }
    else
      str.gsub('/', File::ALT_SEPARATOR)
    end
  else
    str
  end
end

# 指定した条件で、C#のDLLをビルドしてエラーがないか確認する
def build_dll(cs_files, out, defines, dlls)
  return if cs_files.empty?
  cmd = [
    MCS,
    *dlls.map { |lib| '-r:' + lib },
    "-target:library",
    "-out:#{out}",
    *defines.map { |x| "-define:#{x}" },
    *slash(cs_files)
  ]
  Open3.popen2e(*cmd) do |_stdin, stdout_err, wait_thread|
    output = stdout_err.read
    if wait_thread.value != 0
      puts '=' * 80
      puts "ERROR: In compiling #{out}"
      puts '-' * 80
      puts 'CommandLIne:'
      puts cmd.join(' ')
      puts '-' * 80
      puts 'Output:'
      puts output
      puts '=' * 80
      return true
    end
  end

  false
end

# Assets/Starndard Assets/ToydeaLib/* がおのおの分離しているかを確認する
def compile_standard_assets
  failed = false

  common_src = [
    'Client/Assets/Standard Assets/ToydeaLib/Other/MonoSingleton.cs',
    'Client/Assets/Standard Assets/ToydeaLib/Other/PromiseExtension.cs'
  ]

  target_dirs = [
    'Client/Assets/Standard Assets/ToydeaLib/*'
  ]

  exclude_pattern = ['**/*.meta', '**/MemoryProfilerWindow', '**/AudioManager'] # TODO: 一時的にAudioManagerを排除、そのうちもどす

  FileList[*target_dirs].exclude(*exclude_pattern).each do |dir|
    if File.basename(dir) == 'Other'
      FileList[dir + '/**/*.cs'].exclude('**/*Test*').each do |cs_file|
        puts "Compiling #{File.basename(cs_file)} ..."
        failed |= build_dll [cs_file] + common_src, 'ModuleBuilder/' + File.basename(cs_file) + '.dll', DEFINES, LIBS
      end
    else
      puts "Compiling #{File.basename(dir)} ..."
      cs_files = FileList[dir + '/**/*.cs'].exclude('**/Tests/*.cs', '**/Test/*.cs')
      failed |= build_dll cs_files + common_src, 'ModuleBuilder/' + File.basename(dir) + '.dll', DEFINES, LIBS
    end
  end

  failed
end

# Assets/Starndard Assets/* のDLLを作成する
def make_standard_assets_dll
  puts "Compiling StandardAssets ..."
  cs_files = FileList['Client/Assets/Standard Assets/**/*.cs'].exclude('**/*Test*.cs')
  build_dll cs_files, 'ModuleBuilder/StandardAssets.dll', DEFINES, LIBS
end

# ゲームエンジンが分離されているかを確認する
def compile_rogue
  puts "Compiling Rogue ..."
  cs_files = FileList[
    'Client/Assets/Scripts/Rogue/**/*.cs',
  ]
  libs = [
    'ModuleBuilder/Cfs.dll',
    "#{PROJECT_ROOT}/Client/Assets/Standard Assets/ToydeaLib/GoogleProtocolBuffer/GoogleProtocolBuffer.dll"
  ]
  build_dll cs_files, 'ModuleBuilder/Rogue.dll', [], libs
end

# DfzConsoleビルドできるか確認する
def compile_dfz_console
  Dir.chdir 'Experiment/DfzConsole' do
    puts 'Compiling DfzConsole ...'
    _stdin, stdout, wait_thread = Open3.popen2(XBUILD, 'DfzConsole.sln')
    if wait_thread.value != 0
      puts stdout.read
      return true
    end
  end
  false
end

mkdir_p 'ModuleBuilder'
failed |= compile_standard_assets
# failed |= make_standard_assets_dll
failed |= compile_rogue
failed |= compile_dfz_console

if failed
  puts '*' * 80
  puts "ERROR: いずれかのモジュールの分離性が崩れているか、コンパイルに失敗しました"
  puts '*' * 80
  exit 1
end
