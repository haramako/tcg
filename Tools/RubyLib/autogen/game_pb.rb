# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: game.proto

require 'google/protobuf'

require 'master_pb'
Google::Protobuf::DescriptorPool.generated_pool.build do
  add_message "Game.Card" do
    optional :id, :int32, 1
    optional :card_template_id, :int32, 2
  end
  add_message "Game.FieldInfo" do
    optional :hp, :int32, 1
    optional :power, :int32, 2
    optional :turn, :int32, 3
  end
end

module Game
  Card = Google::Protobuf::DescriptorPool.generated_pool.lookup("Game.Card").msgclass
  FieldInfo = Google::Protobuf::DescriptorPool.generated_pool.lookup("Game.FieldInfo").msgclass
end
