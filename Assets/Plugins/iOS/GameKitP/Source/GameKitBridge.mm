//
//  GameKitBridge.m
//
//  Created by Nathan Reeves on 3/16/2020
//  Copyright © 2020 Nathan Reeves. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <GameKit/GameKit.h>
#import <StoreKit/StoreKit.h>
#include "GameKitP-Swift.h"

#pragma mark - C interface
extern "C" {

    char* _sendStringToUnity() {
        NSString *returnString = [[GameKitHelper shared] SendStringToUnity];
        char* cStringCopy(const char* string);
        return cStringCopy([returnString UTF8String]);
    }
    bool _isAuthenticated()
    {
        return [[GameKitHelper shared] isPlayerAuthenticated];
    }
    void _authenticatePlayer(float show)
    {
        [[GameKitHelper shared] authenticatePlayer:show];
    }
    char* _playerID()
    {
        NSString *returnString = [[GameKitHelper shared] getPlayerID];
        char* cStringCopy(const char* string);
        return cStringCopy([returnString UTF8String]);
    }
    void _showAuthView()
    {
        [[GameKitHelper shared] showAuthView];
    }
    int _playersOnline()
    {
        return (int)[[GameKitHelper shared] getActivity];
    }
    bool _canPurchaseInApp()
    {
        return [[GameKitHelper shared] isAuthorizedForPayments];
    }
    void _showRating()
    {
        [[GameKitHelper shared] ShowRating];
    }
    void _showAchievements()
    {
        [[GameKitHelper shared] showAchievements];
    }
    void _purchaseInApp(char* inAppID)
    {
        [[GameKitHelper shared] PurchaseInApp: [NSString stringWithUTF8String:inAppID]];
    }
    void _showLeaderboard(char* leaderboardID)
    {
        [[GameKitHelper shared] showLeaderboard: [NSString stringWithUTF8String:leaderboardID]];
    }
    void _showAllLeaderboards()
    {
        [[GameKitHelper shared] showAllLeaderboards];
    }
    void _postLeaderboardInt(char* l, int val)
    {
        [[GameKitHelper shared] postLeaderboardInt: val leaderboardID: [NSString stringWithUTF8String:l]];
    }
    void _postLeaderboardFloat(char* l, float val)
    {
        [[GameKitHelper shared] postLeaderboardFloat: val leaderboardID: [NSString stringWithUTF8String:l]];
    }
    void _postAchievement(char* type, double prog)
    {
        [[GameKitHelper shared] PostAchievementProgress:[NSString stringWithUTF8String: type] val: prog];
    }
    void _resetAchievements()
    {
        [[GameKitHelper shared] resetAchievementProgress];
    }
    void _leaveMatch()
    {
        [[GameKitHelper shared] leaveMatch];
    }
    void _findMatch(int maxPlayers, int minPlayers)
    {
        [[GameKitHelper shared] findMatch: maxPlayers minP: minPlayers];
    }
    void _cancelMatchFind()
    {
        [[GameKitHelper shared] cancelMatchFind];
    }
    void _sendData(char* data)
    {
        NSString *d = [NSString stringWithUTF8String:data];
        [[GameKitHelper shared] sendData: d];
    }
    
}

char* cStringCopy(const char* string){
    if (string == NULL){
        return NULL;
    }
    char* res = (char*)malloc(strlen(string)+1);
    strcpy(res, string);
    return res;
}
