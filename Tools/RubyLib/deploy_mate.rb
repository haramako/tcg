# frozen_string_literal: true

require 'fileutils'
require 'json'
require 'open-uri'
require 'net/http'

#
# アプリのダウンロードやアップロードを行う
#
# Windows/OSX でのアプリのデプロイを行うためのもの
#
class DeployMate
  include FileUtils

  def initialize(tag_server, cfsctl)
    @tag_server = tag_server
    @cfsctl = cfsctl
  end

  # 指定したタグのEXEをダウンロードして、パスを返す
  def download_app(tag)
    path = "Downloaded/#{tag}"
    sh "#{@cfsctl} sync #{tag} #{path}" unless File.exist? path
    path
  end

  # 指定したタグのアプリを起動する(Windows/OSXが対象)
  def run_app(path, app_name)
    if RUBY_PLATFORM =~ /darwin/
      chmod 0o755, "#{path}/#{app_name}.app/Contents/MacOS/#{app_name}"
      sh "open #{path}/#{app_name}.app"
    else
      puts path
      puts "start #{path.tr('/', '\\')}\\#{app_name}.exe"
      sh "start #{path.tr('/', '\\')}\\#{app_name}.exe"
    end
  end

  # アプリの情報のリストを取得する
  def app_list
    txt = open("#{@tag_server}/list", &:read)
    json = JSON.parse(txt)
    list = json.map do |a|
      next unless a[0] =~ /^dn-.*-desc$/
      [a[0], (begin
                JSON.parse(a[1])
              rescue
                nil
              end)]
    end.compact
    list.map do |k, v|
      v['name'] = k.gsub(/-desc/, '')
      v
    end
  end

  # プラットフォームやブランチ、ビルド番号を指定してアプリの情報を取得する
  def app_info(platform, branch = 'master', build_num = nil)
    data = app_list.select { |d| d['platform'] == platform && d['branch'] == branch }
    if build_num
      found = data.select { |d| d['build_number'] == build_num }[0]
    else
      found = data.sort { |a, b| b['build_number'] - a['build_number'] }[0]
    end
    found
  end

  # タグサーバーにアップロードする
  def upload(path, tag, desc)
    sh "#{@cfsctl} upload --tag #{tag} -o app.hash #{path}"
    upload_hash tag, 'app.hash', JSON.dump(desc)
  end

  # タグサーバーにファイルをアップロードする
  def upload_hash(tag, hash_filename, desc = nil)
    # puts "CFS Hashのアップロード先 #{tag}"
    client_hash = IO.read(hash_filename)
    res = Net::HTTP.post_form(URI.parse("#{@tag_server}/tags/#{tag}"), 'val' => client_hash)
    raise "Hashのアップロードに失敗しました! #{res}" unless res.is_a? Net::HTTPSuccess

    if desc
      res = Net::HTTP.post_form(URI.parse("#{@tag_server}/tags/#{tag}-desc"), 'val' => desc)
      raise "Descのアップロードに失敗しました! #{res}" unless res.is_a? Net::HTTPSuccess
    end
  end
end
