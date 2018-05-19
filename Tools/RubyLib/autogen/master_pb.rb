# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: master.proto

require 'google/protobuf'

Google::Protobuf::DescriptorPool.generated_pool.build do
  add_message "Master.PbxHeader" do
    map :int_index, :int32, :int32, 1
    map :string_index, :string, :int32, 2
  end
  add_message "Master.I18nMessage" do
    optional :id, :string, 1
    optional :text, :string, 2
    optional :tag, :string, 3
    optional :variant, :string, 4
  end
  add_message "Master.MessageFusion" do
    optional :id1, :string, 1
    optional :id2, :string, 2
    optional :text, :string, 3
  end
  add_message "Master.ConfigInfo" do
    optional :id, :string, 1
    optional :value, :string, 2
    optional :type, :enum, 3, "Master.SettingType"
    optional :spec, :string, 4
  end
  add_message "Master.CardTemplate" do
    optional :id, :int32, 1
    optional :name, :string, 2
    optional :desc, :string, 3
    optional :image_id, :int32, 4
    optional :cost, :int32, 5
    repeated :special_template, :message, 6, "Master.SpecialTemplate"
    repeated :type, :enum, 7, "Master.CardTemplate.CardType"
  end
  add_enum "Master.CardTemplate.CardType" do
    value :Attack, 0
    value :Skill, 1
  end
  add_message "Master.SpecialTemplate" do
    optional :type, :enum, 1, "Master.SpecialType"
    optional :amount, :int32, 2
    repeated :opt, :enum, 3, "Master.SpecialTemplate.Option"
    repeated :card_type, :enum, 8, "Master.CardTemplate.CardType"
    optional :card_id, :int32, 4
    optional :location, :enum, 5, "Master.CardLocation"
    optional :location_to, :enum, 7, "Master.CardLocation"
    optional :counter, :message, 6, "Master.SpecialTemplate.Counter"
  end
  add_message "Master.SpecialTemplate.Counter" do
    optional :type, :enum, 1, "Master.SpecialTemplate.CounterType"
    optional :multiply, :int32, 2
    repeated :card_type, :enum, 3, "Master.CardTemplate.CardType"
    optional :card_id, :int32, 4
  end
  add_enum "Master.SpecialTemplate.Option" do
    value :Random, 0
  end
  add_enum "Master.SpecialTemplate.CounterType" do
    value :UsedInTurn, 0
    value :InHand, 1
    value :Mana, 2
  end
  add_message "Master.StatusInfo" do
    optional :id, :int32, 1
    optional :symbol, :string, 2
    optional :name, :string, 3
    repeated :overwrite, :enum, 4, "Master.CharacterStatus"
    repeated :against, :enum, 5, "Master.CharacterStatus"
    optional :decrement, :bool, 6
    optional :all, :bool, 13
    optional :good, :bool, 7
    optional :bad, :bool, 12
    optional :spec, :string, 11
    optional :desc, :string, 16
    optional :without_player, :bool, 18
    optional :without_monster, :bool, 19
  end
  add_message "Master.DebugMenuInfo" do
    optional :name, :string, 1
    optional :action, :string, 2
    optional :shortcut, :string, 4
    repeated :children, :message, 3, "Master.DebugMenuInfo"
  end
  add_enum "Master.SettingType" do
    value :Integer, 0
    value :String, 1
    value :Float, 2
  end
  add_enum "Master.PlatformType" do
    value :Windows, 0
    value :Switch, 1
    value :Ps4, 2
    value :Wsa, 3
    value :Steam, 4
  end
  add_enum "Master.CardLocation" do
    value :NoneCardLocation, 0
    value :Hand, 1
    value :Stack, 2
    value :Grave, 3
  end
  add_enum "Master.SpecialType" do
    value :Attack, 0
    value :Defense, 1
    value :Draw, 2
    value :Weak, 3
    value :PowerUp, 5
    value :PowerDown, 6
    value :AddCard, 7
    value :Discard, 8
    value :CloneCard, 9
    value :AddMana, 10
    value :AddHp, 11
    value :MoveSelectedCard, 12
    value :IfCardAll, 100
  end
  add_enum "Master.CharacterStatus" do
    value :NoneCharacterStatus, 0
    value :Block, 1
  end
  add_enum "Master.StatusGroup" do
    value :NoStatusGroup, 0
    value :AllStatus, 1
    value :GoodStatus, 2
    value :BadStatus, 3
  end
end

module Master
  PbxHeader = Google::Protobuf::DescriptorPool.generated_pool.lookup("Master.PbxHeader").msgclass
  I18nMessage = Google::Protobuf::DescriptorPool.generated_pool.lookup("Master.I18nMessage").msgclass
  MessageFusion = Google::Protobuf::DescriptorPool.generated_pool.lookup("Master.MessageFusion").msgclass
  ConfigInfo = Google::Protobuf::DescriptorPool.generated_pool.lookup("Master.ConfigInfo").msgclass
  CardTemplate = Google::Protobuf::DescriptorPool.generated_pool.lookup("Master.CardTemplate").msgclass
  CardTemplate::CardType = Google::Protobuf::DescriptorPool.generated_pool.lookup("Master.CardTemplate.CardType").enummodule
  SpecialTemplate = Google::Protobuf::DescriptorPool.generated_pool.lookup("Master.SpecialTemplate").msgclass
  SpecialTemplate::Counter = Google::Protobuf::DescriptorPool.generated_pool.lookup("Master.SpecialTemplate.Counter").msgclass
  SpecialTemplate::Option = Google::Protobuf::DescriptorPool.generated_pool.lookup("Master.SpecialTemplate.Option").enummodule
  SpecialTemplate::CounterType = Google::Protobuf::DescriptorPool.generated_pool.lookup("Master.SpecialTemplate.CounterType").enummodule
  StatusInfo = Google::Protobuf::DescriptorPool.generated_pool.lookup("Master.StatusInfo").msgclass
  DebugMenuInfo = Google::Protobuf::DescriptorPool.generated_pool.lookup("Master.DebugMenuInfo").msgclass
  SettingType = Google::Protobuf::DescriptorPool.generated_pool.lookup("Master.SettingType").enummodule
  PlatformType = Google::Protobuf::DescriptorPool.generated_pool.lookup("Master.PlatformType").enummodule
  CardLocation = Google::Protobuf::DescriptorPool.generated_pool.lookup("Master.CardLocation").enummodule
  SpecialType = Google::Protobuf::DescriptorPool.generated_pool.lookup("Master.SpecialType").enummodule
  CharacterStatus = Google::Protobuf::DescriptorPool.generated_pool.lookup("Master.CharacterStatus").enummodule
  StatusGroup = Google::Protobuf::DescriptorPool.generated_pool.lookup("Master.StatusGroup").enummodule
end
