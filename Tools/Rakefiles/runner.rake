# frozen_string_literal: true

#=================================
# runner
#=================================

namespace :runner do
  desc '指定したバージョンのEXEをダウンロードして起動する'
  task :start_exe do
    platform = if RUBY_PLATFORM =~ /darwin/
                 'OSX'
               else
                 'StandaloneWindows'
               end
    branch = ENV['BRANCH'] || 'master'
    build_number = ENV['BUILD_NUMBER'] && ENV['BUILD_NUMBER'].to_i
    tag = ENV['TAG']
    raise 'TAGもしくはBRANCHが指定されていません' unless tag || branch

    unless tag
      info = DEPLOY_MATE.app_info(platform, branch, build_number)
      raise '指定されたバージョンのアプリがみつかりません' unless info
      pp info
      tag = info['name']
    end

    path = DEPLOY_MATE.download_app(tag)
    logger.info "#{tag} を起動します"
    logger.info '「アクセスを許可する」を押してください'
    DEPLOY_MATE.run_app(path, 'doggieninja')
  end
end
