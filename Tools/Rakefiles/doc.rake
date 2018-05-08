# coding: utf-8
# frozen_string_literal: true

# ドキュメント作成系のタスク

namespace :doc do
  task :jekyll do
    chdir 'Doc' do
      sh 'LANG=ja_JP.utf-8 bundle exec jekyll build'
    end
  end

  desc 'Doxygenのドキュメントの作成をする'
  task :doxygen do
    mkdir_p 'Doc/_site/AutoGen'
    sh 'doxygen'
  end

  desc 'ProtocolBufferのドキュメントを作成する'
  task proto: :proto_doc

  desc 'ドキュメントをtddl01にアップロードする'
  task :upload do
    if RUBY_PLATFORM =~ /darwin/
      # Macのファイルシステムが濁点などの形式が違う問題の解決のために iconv オプションを指定している
      # 最新のrsyncをHomebrewで入れること
      # See: http://qiita.com/knaka/items/48e1799b56d520af6a09
      sh 'rsync --iconv=UTF8-MAC,UTF-8 -avz Doc/_site/ tdadmin@133.242.235.150:/home/tdadmin/doggie-ninja/'
    else
      sh 'rsync -avz Doc/_site/ tdadmin@133.242.235.150:/home/tdadmin/tcg/'
    end
  end

  desc 'ドキュメントをtddl01にアップロードする'
  task :all => [:'doc:jekyll', :'doc:doxygen', :'doc:proto', :upload]
end
