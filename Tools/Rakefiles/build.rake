# coding: utf-8
# frozen_string_literal: true

# 自動ビルド関係のRakefile

require 'json'

BUILD_VERSION = '1.0.4'
CABINET_SERVER = 'http://133.242.235.150:7000' # キャビネット情報が置いてあるサーバー
GATE_KEEPER_SERVER = 'http://tdadmin:phi3zooJ@133.242.235.150:3030/' # ゲートキーパーのサーバー

TARGET_PLATFORM_ALIASES = {
  'win' => 'StandaloneWindows',
  'windows' => 'StandaloneWindows',
  'standalonewindows' => 'StandaloneWindows',
  'ios' => 'iOS',
  'android' => 'Android'
}.freeze

COMMANDLINE_BUILD_TARGETS = {
  'StandaloneWindows' => 'win64',
  'iOS' => 'ios',
  'Android' => 'android'
}.freeze

TARGET_PLATFORMS = %w[
  StandaloneWindows64
  Android
  iOS
].freeze

#==========================================================
# 便利関数群
#==========================================================

def get_target_platform(name)
  platform = TARGET_PLATFORM_ALIASES[name.downcase]
  if platform
    platform
  else
    raise "不正なプラットフォームです, platform=#{name}。 有効な指定は #{TARGET_PLATFORM_ALIASES.values.join(',')} です"
  end
end

# デプロイゲートにファイルをアップロードする
def upload_deploygate(path, message)
  require 'net/http'
  require 'openssl'

  # WindowsのRuby2.3でSSLのキーがおかしいので、ここで証明書を手動で設定する
  url = URI.parse("https://deploygate.com/api/users/#{DEPLOYGATE_USER}/apps")
  https = Net::HTTP.new(url.host, 443)
  https.use_ssl = true
  https.ca_file = './cacert.pem'

  https.start do |s|
    req = Net::HTTP::Post.new(url.path)
    file = open(path, 'rb')
    req.set_form({ 'token' => DEPLOYGATE_TOKEN, 'file' => file, 'message' => message }, 'multipart/form-data')
    res = s.request(req)
    unless res.is_a? Net::HTTPSuccess
      puts res.body
      raise "アップロードに失敗しました! #{res}"
    end
  end
end

#==========================================================
# タスク
#==========================================================

PROJ_DIR = 'DoggieNinja'
# rubocop: disable Metrics/BlockLength
namespace :build do
  desc 'アプリをビルドする(PLATFORM=対象のプラットフォーム, BUILD=Debug|Release|Final)'
  task :app do
    # build_number = (ENV['BUILD_NUMBER'] || 0).to_i
    platform = get_target_platform(ENV['PLATFORM'] || 'windows')
    build = ENV['BUILD'] || 'Debug'

    unless FileTest.exist?('build.log')
      f = open('build.log', 'w')
      f.close
    end

    # 不要なアセバンを削除する
    FileList["#{PROJ_DIR}/Assets/StreamingAssets/*"].each do |fname|
      basename = File.basename(fname)
      rm_rf fname if TARGET_PLATFORMS.include?(basename) && basename != platform
    end

    # 不要なResourcesを削除する
    FileList["#{PROJ_DIR}/Assets/Resources/*"].each do |fname|
      next if fname =~ /\.txt/
      rm_rf fname
    end

    puts "Platform = #{platform}"

    # 作成済みのファイルを消す
    rm_rf "#{PROJ_DIR}/AppBuilder"

    # Config設定
    # commit = `git rev-parse --verify HEAD`

    rm_f "#{PROJ_DIR}/AppBuilderOutput.txt" # 出力ファイルを削除する

    case ENV['KEYSTORE']
    when 'kaser'
      package_opt = [
        '-keystorePath', File.absolute_path("#{PROJ_DIR}/Platforms/Android/com.kaser.dgtest.keystore"),
        '-keystorePass', 'td121001',
        '-aliasName', 'dgtest',
        '-aliasPass', 'td121001'
      ]
    else
      package_opt = [
        '-identifier', 'com.toydea.doggieninja',
        '-keystorePath', File.absolute_path("#{PROJ_DIR}/Platforms/Android/com.toydea.doggieninja.keystore"),
        '-keystorePass', 'phi3zooJ',
        '-aliasName', 'doggieninja',
        '-aliasPass', 'phi3zooJ'
      ]
    end

    sh(UNITY_EXE,
       '-batchMode',
       '-quit',
       '-projectPath', File.absolute_path("#{__dir__}/#{PROJ_DIR}"),
       '-buildTarget', COMMANDLINE_BUILD_TARGETS[platform],
       '-logFile', 'build.log',
       '-executeMethod', 'AppBuilder.Build',
       '-target', platform,
       '-build', build,
       '-outputpath', 'AppBuilder',
       *package_opt) do |ok, _status|
      unless ok
        puts IO.read('build.log')
        raise
      end
    end

    unless File.exist? "#{PROJ_DIR}/AppBuilderOutput.txt"
      puts IO.read('build.log')
      raise 'アプリのビルドに失敗しました'
    end
  end

  desc 'IOSのxcarchiveを作成する'
  task :ios_archive do
    platform = get_target_platform(ENV['PLATFORM'])
    raise 'iOS以外のプラットフォームでは無効です' if platform != 'iOS'
    XCODE_PROJ = "#{PROJ_DIR}/AppBuilder/iOS/doggieninja/Unity-iPhone.xcodeproj"

    # keychainを使えるようにする
    # See: https://stackoverflow.com/questions/43632856/unknown-error-1-ffffffffffffffff-command-bin-sh-failed-with-exit-code-1-in-jen
    PASS = 'mako0522'
    sh "security set-key-partition-list -S apple-tool:,apple: -s -k #{PASS} #{ENV['HOME']}/Library/Keychains/login.keychain-db"

    sh('xcodebuild',
       'archive',
       '-project', XCODE_PROJ,
       '-scheme', 'Unity-iPhone',
       '-archivePath', 'output',
       'DEVELOPMENT_TEAM=8QC965MT7U',
       'ENABLE_BITCODE=NO')
  end

  desc 'iOSのipaファイルを作成する'
  task :ios_ipa do
    platform = get_target_platform(ENV['PLATFORM'])
    raise 'iOS以外のプラットフォームでは無効です' if platform != 'iOS'
    sh('xcodebuild',
       '-exportArchive',
       '-archivePath', 'output.xcarchive',
       '-exportPath', 'xcoutput',
       '-exportOptionsPlist', 'Tools/RakeTask/ExportOptions.plist')
    cp 'xcoutput/Unity-iPhone.ipa', 'doggieninja.ipa'
  end

  desc 'DeployGateにアップロードする'
  task :deploy_gate do
    platform = get_target_platform(ENV['PLATFORM'] || 'windows')

    build_number = (ENV['BUILD_NUMBER'] || 0).to_i
    commit_id = `git show -s --format=%H`.strip
    branch = (ENV['GIT_BRANCH'] || `git rev-parse --abbrev-ref HEAD`.strip).gsub(%r{^origin/}, '')
    git_log = `git log --date=iso -n 10 --pretty=format:"[%ad] %s"`
    git_log = git_log.force_encoding(Encoding::UTF_8) # Windows上で文字コードがおかしいのを修正
    tag = 'dn-app-' + platform + '-' + commit_id[0, 8]
    deploy_gate_msg = "Branch:#{branch.tr('/', '_')} Build:#{build_number} Commit:#{commit_id[0, 8]}"

    case platform
    when 'Android'
      upload_deploygate("#{PROJ_DIR}/AppBuilder/Android/doggieninja.apk", deploy_gate_msg)
    when 'iOS'
      upload_deploygate('doggieninja.ipa', deploy_gate_msg)
    when 'StandaloneWindows'
      desc = {
        build_number: build_number,
        branch: branch,
        date: Time.now.to_s,
        commit_id: commit_id,
        platform: platform,
        log: git_log
      }
      path = "#{PROJ_DIR}/AppBuilder/#{platform}"
      DEPLOY_MATE.upload(path, tag, desc)
    else
      raise "不正なプラットフォームです #{platform}"
    end
  end
end
