# frozen_string_literal: true

require 'net/http'
require 'json'

class SteamTools
  ROOT_URL = 'https://partner.steam-api.com/ISteamApps/'

  def initialize(api_key_, app_id_)
    @api_key = api_key_
    @app_id = app_id_
  end

  def get(path, params = {})
    params = params.merge(key: @api_key, appid: @app_id)
    uri = URI.parse(ROOT_URL + path)
    uri.query = URI.encode_www_form(params)
    req = Net::HTTP::Get.new(uri)
    http = Net::HTTP.new(uri.host, uri.port)
    http.use_ssl = true
    res = http.start do
      http.request req
    end
    JSON.parse(res.body, symbolize_names: true)
  end

  def post(path, params = {})
    params = params.merge(key: @api_key, appid: @app_id)
    uri = URI.parse(ROOT_URL + path)
    req = Net::HTTP::Post.new(uri)
    req.set_form_data(params)
    http = Net::HTTP.new(uri.host, uri.port)
    http.use_ssl = true
    res = http.start do
      http.request req
    end
    JSON.parse(res.body, symbolize_names: true)
  end
end
