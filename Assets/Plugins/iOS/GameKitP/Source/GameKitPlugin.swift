//
//  GameKitPlugin.swift
//  GameKitP
//
//  Created by Nathan Reeves on 7/26/19.
//  Copyright © 2019 Nathan Reeves. All rights reserved.
//
//

import Foundation
import UIKit
import GameKit
import StoreKit

@objc public class GameKitHelper: UIViewController, GKMatchDelegate, GKGameCenterControllerDelegate, SKProductsRequestDelegate, SKPaymentTransactionObserver, GKMatchmakerViewControllerDelegate, GKLocalPlayerListener {
    
    @objc static let shared = GameKitHelper()
    @objc var thisMatch: GKMatch?
    var matching: Bool = false
    var productRequest: SKProductsRequest!
    @objc var thismatchMaker: GKMatchmaker?
    @objc var thisPlayer: GKLocalPlayer?
    @objc var authenticationViewController: UIViewController!
    @objc var GameKitViewController: UIViewController!
    var oldBrightness: Double = 0.0
    
    //Callbacks in Unity to "GameKit.cs"
    let kCallbackTarget = "GameKit"
    
    
    ////
    //// Authenticating GameCenter Player
    ////

    
    //Returns to Unity if player is authenticated
    @objc func authenticatePlayer(_ show: Int) -> Void
    {
        SKPaymentQueue.default().add(self)
        thisPlayer = GKLocalPlayer.local
        self.thisPlayer!.authenticateHandler =  { (viewController : UIViewController!, error : Error!) -> Void in
            if(viewController != nil)
            {
                self.authenticationViewController = viewController
            }
            if(error != nil)
            {
                print(error);
            }
            if(self.thisPlayer!.isAuthenticated)
            {
                self.thisPlayer!.register(self)
                UnitySendMessage(self.kCallbackTarget, "PlayerAuthenticated", "true")
            }
            else
            {
                UnitySendMessage(self.kCallbackTarget, "PlayerAuthenticated", "false")
            }
        }
        
        let notificationCenter = NotificationCenter.default
        notificationCenter.addObserver(self, selector: #selector(appMovedToBackground), name: UIApplication.didEnterBackgroundNotification, object: nil)
        notificationCenter.addObserver(self, selector: #selector(appisMovingToBackground), name: UIApplication.willResignActiveNotification, object: nil)
    }
    
    //Show GameCenter login prompt if not authenticated
    @objc func showAuthView() -> Void
    {
        if(self.authenticationViewController != nil)
        {
            let vc = UIApplication.shared.keyWindow?.rootViewController;
            vc!.present(self.authenticationViewController, animated: true)
        }
    }
    @objc func isPlayerAuthenticated() -> Bool
    {
        return self.thisPlayer!.isAuthenticated
    }
    
    // Player ID used for deciding who will be host
    @objc func getPlayerID() -> String
    {
        return thisPlayer?.playerID ?? "";
    }

    
    ////
    ////    Matchmaking
    ////
    //Open MatchmakerViewController to find a match with 2 players
    @objc func findMatch(_ maxP: Int, minP: Int) -> Void
    {
        let request = GKMatchRequest()
        request.maxPlayers = maxP;
        request.minPlayers = minP;
        matching = true
        let vc = UIApplication.shared.keyWindow?.rootViewController;
        var matchViewController: GKMatchmakerViewController = GKMatchmakerViewController.init(matchRequest: request)!
        matchViewController.matchmakerDelegate = self
        self.GameKitViewController = matchViewController

        vc!.present(matchViewController, animated: true, completion: nil)
        
        /*
         //For implementing a custom matchmaking view (no ViewController)
        thismatchMaker =  GKMatchmaker()
        thismatchMaker!.findMatch(for: request) { (match, err) -> Void in
            if(err != nil)
            {
                print("error finding match")
            }
            else if(match != nil)
            {
                self.thisMatch = match
                match!.delegate = GameKitHelper.shared
                print("Match found")
                UnitySendMessage(self.kCallbackTarget, "FoundMatch", "")
                print(match!.expectedPlayerCount)
            }
        }
        */
    }
    
    //Match Found (not accepted invite)
    //returns matched players ID
    public func matchmakerViewController(_ viewController: GKMatchmakerViewController,
    didFind match: GKMatch)
    {
        self.thisMatch = match
        match.delegate = GameKitHelper.shared
        UnitySendMessage(self.kCallbackTarget, "FoundMatch", match.players[0].playerID)
        viewController.dismiss(animated: true, completion: nil)
        if(self.GameKitViewController != nil)
        {
            self.GameKitViewController.dismiss(animated: true, completion: nil)
        }
        
    }
    
    //When Local Player accepts match invite
    //returns other players ID
    public func player(_ player: GKPlayer, didAccept invite: GKInvite) {
        
        if(thismatchMaker == nil) {thismatchMaker =  GKMatchmaker.shared()}
        
        thismatchMaker!.match(for: invite) {(match, err) -> Void in
            self.thisMatch = match
            match!.delegate = GameKitHelper.shared
            UnitySendMessage(self.kCallbackTarget, "FoundMatch", "i"+invite.sender.playerID)
            
            if(self.GameKitViewController != nil)
            {
                self.GameKitViewController.dismiss(animated: true, completion: nil)
            }
        }
      
    }
    
    //Cancel Matchmaking search
    @objc func cancelMatchFind() -> Void
    {
        if(thismatchMaker != nil){thismatchMaker!.cancel()}
    }
    
    //Callback to Unity when MatchmakerView is closed
    public func matchmakerViewControllerWasCancelled(_ viewController: GKMatchmakerViewController) {
        DispatchQueue.main.asyncAfter(deadline: .now() + 0.2) {
            self.matching = false
            viewController.dismiss(animated: true, completion: nil)
            if(self.GameKitViewController != nil)
            {
                self.GameKitViewController.dismiss(animated: true, completion: nil)
            }
            UnitySendMessage(self.kCallbackTarget, "CancelMatchmaking", "1")
        }
        
    }
    //MatchmakerView is closed on failure
    public func matchmakerViewController(_ viewController: GKMatchmakerViewController, didFailWithError error: Error) {
        viewController.dismiss(animated: true, completion: nil)
        if(self.GameKitViewController != nil)
        {
            self.GameKitViewController.dismiss(animated: true, completion: nil)
        }
    }
    //Close gamecenter views on finish
    public func gameCenterViewControllerDidFinish(_ gameCenterViewController: GKGameCenterViewController) {
        gameCenterViewController.dismiss(animated: true, completion: nil)
        if(self.GameKitViewController != nil)
        {
            self.GameKitViewController.dismiss(animated: true, completion: nil)
        }
    }


    ////
    ////    In Match
    ////
    
    //Connection Status
    public func match(_ match: GKMatch, player: GKPlayer, didChange state: GKPlayerConnectionState)
    {
        if(state.rawValue == 1)
        {
            print("player CONNECTED");
        }
        else if(state.rawValue == 2)
        {
            //Unity callback when enemy disconnects
            UnitySendMessage(self.kCallbackTarget, "EnemyLeftMatch", "")
        }
        else
        {
            print("player CONNECTION STATE UNK");
        }
    }
    
    // Send data to all players
    @objc func sendData(_ data: String) -> Void
    {
        let packet = data.data(using: .utf8)
        do{
            try thisMatch?.sendData(toAllPlayers: packet!, with: .reliable)
        }
        catch{/*error*/}
    }
    
    //Receive data from player
    public func match(_ match: GKMatch, didReceive data: Data, fromRemotePlayer player: GKPlayer) {
        
        let str = String(data: data, encoding: .utf8)
        UnitySendMessage(self.kCallbackTarget, "ReceivedData", str)

    }
    @objc func leaveMatch() -> Void
    {
        if(self.thisMatch != nil)
        {
            self.thisMatch!.disconnect()
            self.thisMatch = nil
        }
    }
    
    
    
    
    ////
    ////    GameCenter Achievements
    ////
    @objc func resetAchievementProgress() -> Void
    {
        GKAchievement.resetAchievements(completionHandler: {(error) in
            print("Achievements reset")
        })
    }
    @objc func PostAchievementProgress(_ id: String, val: Double) -> Void
    {
        let challenge = GKAchievement.init(identifier: id)
        challenge.percentComplete = val
        challenge.showsCompletionBanner = true
        GKAchievement.report([challenge], withCompletionHandler: {(error) in
            print("Achievement posted", id)
        })
    }
    @objc func showAchievements() -> Void
    {
        let vc = UIApplication.shared.keyWindow?.rootViewController
        var gcViewController: GKGameCenterViewController = GKGameCenterViewController()
        gcViewController.gameCenterDelegate = self

        gcViewController.viewState = GKGameCenterViewControllerState.achievements

        vc!.present(gcViewController, animated: true, completion: nil)
    }

    
    ////
    ////    GameCenter Leaderboard
    ////
    @objc func showLeaderboard(_ id: String) -> Void
    {
        let vc = UIApplication.shared.keyWindow?.rootViewController
        var gcViewController: GKGameCenterViewController = GKGameCenterViewController()
        gcViewController.gameCenterDelegate = self

        gcViewController.viewState = GKGameCenterViewControllerState.leaderboards
        gcViewController.leaderboardIdentifier = id

        vc!.present(gcViewController, animated: true, completion: nil)
    }
    @objc func showAllLeaderboards() -> Void
    {
        let vc = UIApplication.shared.keyWindow?.rootViewController
        var gcViewController: GKGameCenterViewController = GKGameCenterViewController()
        gcViewController.gameCenterDelegate = self

        gcViewController.viewState = GKGameCenterViewControllerState.leaderboards

        vc!.present(gcViewController, animated: true, completion: nil)
    }
    @objc func postLeaderboardFloat(_ floatScore: Float, leaderboardID: String) -> Void
    {
        let bestScoreInt = GKScore(leaderboardIdentifier: leaderboardID)
        bestScoreInt.value = Int64(floatScore * 100)
        GKScore.report([bestScoreInt]) { (error) in
            if error != nil {
                print(error!.localizedDescription)
            } else {
                //print("Posting Leaderboard Score")
            }
        }
    }
    @objc func postLeaderboardInt(_ intScore: Int, leaderboardID: String) -> Void
    {
        let bestScoreInt = GKScore(leaderboardIdentifier: leaderboardID)
        bestScoreInt.value = Int64(intScore)
        GKScore.report([bestScoreInt]) { (error) in
            if error != nil {
                print(error!.localizedDescription)
            } else {
                //print("Posting Leaderboard Score")
            }
        }
    }
    
    
       
    
    ////
    ////    In App Purchases
    ////
    public func paymentQueue(_ queue: SKPaymentQueue, shouldAddStorePayment payment: SKPayment, for product: SKProduct) -> Bool {
           return true
       }
    public func paymentQueue(_ queue: SKPaymentQueue, updatedTransactions transactions: [SKPaymentTransaction]) {
        for transaction in transactions {
         switch transaction.transactionState {
         case .purchasing: print("purchasing")
         case .failed: failedPurchase(transaction: transaction)
         case .purchased:  complete(transaction: transaction)
         @unknown default: print("Unexpected transaction state \(transaction.transactionState)")
        }
         }
    }
    private func failedPurchase(transaction: SKPaymentTransaction) {
        print("purchase failed")
        UnitySendMessage(self.kCallbackTarget, "InAppPurchased", "failed")
        SKPaymentQueue.default().finishTransaction(transaction)
    }
    private func complete(transaction: SKPaymentTransaction) {
        print("purchase complete...")
        UnitySendMessage(self.kCallbackTarget, "InAppPurchased", "complete")
        SKPaymentQueue.default().finishTransaction(transaction)
    }
    public func productsRequest(_ request: SKProductsRequest, didReceive response: SKProductsResponse) {
        for r in response.products
        {
            self.buyProduct(r)
        }
    }
    fileprivate func fetchProducts(matchingIdentifiers identifiers: [String]) {
         // Create a set for the product identifiers.
         let productIdentifiers = Set(identifiers)

         // Initialize the product request with the above identifiers.
         productRequest = SKProductsRequest(productIdentifiers: productIdentifiers)
         productRequest.delegate = self

         // Send the request to the App Store.
         productRequest.start()
    }
    @objc var isAuthorizedForPayments: Bool {
        return SKPaymentQueue.canMakePayments()
    }
    public func buyProduct(_ product: SKProduct) {
      print("Buying \(product.productIdentifier)...")
      let payment = SKPayment(product: product)
      SKPaymentQueue.default().add(payment)
    }
    
    @objc func PurchaseInApp(_ id: String) -> Void
    {
        self.fetchProducts(matchingIdentifiers: [id])
    }
    
    ////
    ////  Helpful fuctions
    ////
    
    //  Detecting when Lock Button pressed-
    //  Player will leave the match without GameCenter knowing, handle this in Unity
    @objc func appMovedToBackground() {
        if(self.DidUserPressLockButtonEnd())
        {
            UnitySendMessage(self.kCallbackTarget, "LockButtonPressed", "")
        }
    }
    @objc func appisMovingToBackground() {
          self.DidUserPressLockButton()
      }
    func DidUserPressLockButtonEnd() -> Bool {
        let b = UIScreen.main
        return oldBrightness == Double(b.brightness)
    }
    func DidUserPressLockButton() -> Bool {
        let b = UIScreen.main
        oldBrightness = Double(b.brightness)
        b.brightness = CGFloat(oldBrightness + (oldBrightness <= 0.01 ? (0.01) : (-0.01)))
       return oldBrightness == Double(b.brightness)
    }
    
    //Displays prompt to request user to rate the app
    @objc func ShowRating() -> Void
    {
        if #available(iOS 10.3, *) {
            SKStoreReviewController.requestReview()
        } else {
        
        }
    }
    
    //Returns the number of players recently searching for a match
    @objc func getActivity() -> Int
    {
        if(thismatchMaker == nil) {thismatchMaker =  GKMatchmaker.shared()}
        var playersOnline = 0
        thismatchMaker!.queryActivity() {(activity, err) -> Void in
            playersOnline = activity
            UnitySendMessage(self.kCallbackTarget, "PlayersOnline", String(playersOnline))
        }
        
        return playersOnline
    }

    @objc func SendStringToUnity() -> String{
        return "hi"
    }
    
    /*
     //Return list of leaderboard scores
    @objc func leaderboardDistance() -> Array<float>
    {
        var scorecount = 1;
        let leaderboard = GKLeaderboard()
        leaderboard.playerScope = .global
        leaderboard.timeScope = .alltime
        leaderboard.identifier = "distance"
        leaderboard.range = NSMakeRange(1, 100)
        var sc = [float]()
        leaderboard.loadScores { scores, error in
            guard let scores = scores else { return }
            //print(scores.count)
            for score in scores {
                sc.append(score.value)
                print(score.value)
                print(score.player.alias)
            }
            //UnitySendMessage(self.kCallbackTarget, "PlayersOnline", String(ponline))
        }
        return scores;
    }*/

}
