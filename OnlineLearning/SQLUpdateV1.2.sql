-- for customers who already installed previous version, update database by running the query below
-- add some columns to ChatChannel table
ALTER TABLE ChatChannel ADD UserOneClearedHistory bit NOT NULL default(0), UserTwoClearedHistory bit NOT NULL default(0),
UserOneDeletedChat bit NOT NULL default(0), UserTwoDeletedChat bit NOT NULL default(0),
UserOneClearedHistoryTime datetime NULL,UserTwoClearedHistoryTime datetime NULL,
UserOneDeletedChatTime datetime NULL, UserTwoDeletedChatTime datetime NULL

