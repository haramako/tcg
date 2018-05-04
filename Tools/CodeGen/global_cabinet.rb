# coding: utf-8
# frozen_string_literal: true

# グローバルなG

namespace :Game do
  cabinet :G do
    table :ThinkingType
    table :ExpOfLevel
    table :DungeonStage
    table :ItemSet
    table :ConfigInfo, no_index: true
    table :Skill
    table :TestGame
    table :CharacterAppearance
    table :ImagePosition, type: :PositionTemplate
    table :NewsInfo
    table :TutorialDialog

    table :I18nMessage, no_index: true, no_load: true do
      group :Id, :string
    end

    table :MessageFusion, no_index: true, no_load: true do
      index :Id1, :string, no_dict: true
    end

    table :StatusInfo, type: :StatusInfo do
      index :Symbol, :string, no_dict: true
    end

    table :Item, type: :ItemTemplate do
      on_loaded
      index :Name, :string, no_dict: true
    end

    table :Character, type: :CharacterTemplate do
      on_loaded
      index :Name, :string, no_dict: true
    end

    table :Stage do
      index :Symbol, :string
    end

    table :MetaMapTmx do
      index :Symbol, :string
    end

    table :Dungeon do
      on_loaded
    end

    table :SkillEffect do
      index :Symbol, :string
    end

    table :DebugMenuInfo, no_index: true do
      index :Name, :string
    end

    table :MetaMapSet, no_index: true do
      group :Id, :string
    end

    table :LobbySet, no_index: true do
      group :Id, :string
    end

    table :UnrevealName do
      group :ItemType, :ItemType
    end

    table :Achievement do
    end
  end
end
emit 'using Master;'
