# coding: utf-8
# frozen_string_literal: true

emit 'using Master;'

namespace :Game do
  cs_class :Character do
    property :MaxHp, :int, dirty: true
    property :Attack, :int, dirty: true
    property :Defense, :int, dirty: true
    property :MoveType, :MoveType, dirty: true
  end

  cs_class :PlayerInfo do
    property :MaxHp, :int, dirty: true
    property :Attack, :int, dirty: true
    property :Defense, :int, dirty: true
    property :ArrowAttack, :int, dirty: true
    property :MaxArrow, :int, dirty: true
    property :MaxItem, :int, dirty: true
    property :MaxBrave, :int, dirty: true
    property :MaxTime, :int, dirty: true
    property :Souls, 'List<Soul>', dirty: true
  end
end
