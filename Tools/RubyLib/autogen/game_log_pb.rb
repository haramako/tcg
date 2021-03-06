# Generated by the protocol buffer compiler.  DO NOT EDIT!
# source: game_log.proto

require 'google/protobuf'

require 'master_pb'
Google::Protobuf::DescriptorPool.generated_pool.build do
  add_message "GameLog.Hoge" do
    optional :id, :int32, 1
  end
  add_message "GameLog.Redraw" do
  end
  add_message "GameLog.ShutdownRequest" do
  end
  add_message "GameLog.AckResponseRequest" do
  end
  add_message "GameLog.TurnEndRequest" do
  end
  add_message "GameLog.PlayCardRequest" do
    optional :card_id, :int32, 1
  end
  add_message "GameLog.ShowMessage" do
    optional :text, :string, 1
  end
  add_message "GameLog.DrawCardRequest" do
  end
  add_message "GameLog.CardPlayed" do
  end
  add_message "GameLog.FocusCard" do
    optional :card_id, :int32, 1
  end
  add_message "GameLog.SelectCard" do
    optional :out_card_id, :int32, 1
  end
  add_message "GameLog.AddCard" do
    optional :card_id, :int32, 1
  end
end

module GameLog
  Hoge = Google::Protobuf::DescriptorPool.generated_pool.lookup("GameLog.Hoge").msgclass
  Redraw = Google::Protobuf::DescriptorPool.generated_pool.lookup("GameLog.Redraw").msgclass
  ShutdownRequest = Google::Protobuf::DescriptorPool.generated_pool.lookup("GameLog.ShutdownRequest").msgclass
  AckResponseRequest = Google::Protobuf::DescriptorPool.generated_pool.lookup("GameLog.AckResponseRequest").msgclass
  TurnEndRequest = Google::Protobuf::DescriptorPool.generated_pool.lookup("GameLog.TurnEndRequest").msgclass
  PlayCardRequest = Google::Protobuf::DescriptorPool.generated_pool.lookup("GameLog.PlayCardRequest").msgclass
  ShowMessage = Google::Protobuf::DescriptorPool.generated_pool.lookup("GameLog.ShowMessage").msgclass
  DrawCardRequest = Google::Protobuf::DescriptorPool.generated_pool.lookup("GameLog.DrawCardRequest").msgclass
  CardPlayed = Google::Protobuf::DescriptorPool.generated_pool.lookup("GameLog.CardPlayed").msgclass
  FocusCard = Google::Protobuf::DescriptorPool.generated_pool.lookup("GameLog.FocusCard").msgclass
  SelectCard = Google::Protobuf::DescriptorPool.generated_pool.lookup("GameLog.SelectCard").msgclass
  AddCard = Google::Protobuf::DescriptorPool.generated_pool.lookup("GameLog.AddCard").msgclass
end
