# coding: utf-8
# frozen_string_literal: true

# グローバルなG

namespace :Game do
  cabinet :G do
    table :I18nMessage, no_index: true, no_load: true do
      group :Id, :string
    end

    table :MessageFusion, no_index: true, no_load: true do
      index :Id1, :string, no_dict: true
    end

    table :CardTemplate do
      on_loaded
    end

    table :StatusInfo
    table :ConfigInfo, no_index: true do
      index :Id, :string
    end
  end
end

emit 'using Master;'
