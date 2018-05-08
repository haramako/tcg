# coding: utf-8
# frozen_string_literal: true

# グローバルなG

namespace :Game do
  cabinet :G do
    table :CardTemplate

    table :I18nMessage, no_index: true, no_load: true do
      group :Id, :string
    end

    table :MessageFusion, no_index: true, no_load: true do
      index :Id1, :string, no_dict: true
    end

  end
end

emit 'using Master;'
