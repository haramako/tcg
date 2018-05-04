# coding: utf-8
# frozen_string_literal: true

# いろいろのもののPathを取得する

def detect_path(pattern)
  require 'rake'
  files = FileList[pattern]
  if files.empty?
    puts "Warning: program not found #{pattern}"
    nil
  else
    file = files.select { |x| File.exist?(x) }.sort.last
    if file
      # puts "Detected #{file}"
      file
    else
      puts "Warning: program not found #{pattern}"
      nil
    end
  end
end

if RUBY_PLATFORM =~ /darwin/
  UNITY = detect_path('/Applications/Unity2017.2.1p2/Unity.app/Contents')
  UNITY_EXE = UNITY + '/MacOS/Unity'
  MCS = 'mcs'
  XBUILD = 'xbuild'
  MONO = ['mono', '--debug']
else
  UNITY = detect_path(['D:/Program Files/UnityNx2017.2.1p2/Editor/Data', 'C:/Program Files/UnityNx2017.2.1p2/Editor/Data', 'C:/Program Files/Unity2017.2.1p2/Editor/Data', 'D:/Program Files/Unity2017.2.1p2/Editor/Data', 'C:/Program Files/Unity/Editor/Data'])
  UNITY_EXE = UNITY.to_s + '/../Unity.exe'
  MCS = detect_path('C:/Windows/Microsoft.NET/Framework/v4.*/csc.exe')
  XBUILD = detect_path([
                         'C:/Program Files (x86)/MSBuild/*/Bin/MSBuild.exe', # Visual Studio 2015
                         'C:/Program Files (x86)/Microsoft Visual Studio/2017/Community/MSBuild/15.0/Bin/MSBuild.exe', # Visual Studio 2017 Community
                         'C:/Program Files (x86)/Microsoft Visual Studio/2017/BuildTools/MSBuild/*/Bin/MSBuild.exe', # choco install -y microsoft-build-tools
                       ])
  MONO = []
end
