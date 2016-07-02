<?php

  require_once ('TwitterOj.php');
  ini_set("display_errors", 1);

  define('KEY', 'X');
  define('KEYSECRET', 'X');

  define('dbHost', 'localhost');
  define('dbName', 'mattmmdn_oj');
  define('dbUser', 'mattmmdn_oj');
  define('dbPassword', 'X');

  \TwitterOj\TwitterOj::setConsumerKey(KEY, KEYSECRET);
  $TwitterOj = \TwitterOj\TwitterOj::getInstance();


  if (isset($_GET['oauth_verifier'])) {

    $databaseConnection = new PDO("mysql:host=" . dbHost . ";dbname=" .dbName, dbUser, dbPassword);

    $existNot = $databaseConnection->prepare('SELECT * FROM TwitterOj WHERE username_or_email=:username_or_email AND password=:password');
    $existNot->bindParam(':username_or_email', $_GET['username_or_email']);
    $existNot->bindParam(':password', $_GET['password']);
    $existNot->execute();
    $newAccount = $existNot->fetch(PDO::FETCH_ASSOC);

    if(!$newAccount)
    {

      $TwitterOj->setToken($_GET['oauth_token'], $_GET['oauth_token_secret']);

      $accOuth = $TwitterOj->oauth_accessToken([
        'oauth_verifier' => $_GET['oauth_verifier']
      ]);

      $TwitterOj->setToken($accOuth->oauth_token, $accOuth->oauth_token_secret);
      $accountInfo = $TwitterOj->account_verifyCredentials();

      $addAccount = $databaseConnection->prepare("INSERT INTO `TwitterOj` (username_or_email, password, id, screen_name, name, statuses_count, friends_count, followers_count, favourites_count, verified, created_at, profile_image_url_https, oauthtoken, oauthtokensecret) VALUES (:username_or_email, :password, :id, :screen_name, :name, :statuses_count, :friends_count, :followers_count, :favourites_count, :verified, :created_at, :profile_image_url_https, :oauthtoken, :oauthtokensecret)");
      $addAccount->bindParam(':username_or_email', $_GET['username_or_email']);
      $addAccount->bindParam(':password', $_GET['password']);
      $addAccount->bindParam(':id', $accountInfo->id);
      $addAccount->bindParam(':screen_name', $accountInfo->screen_name);
      $addAccount->bindParam(':name', $accountInfo->name);
      $addAccount->bindParam(':statuses_count', $accountInfo->statuses_count);
      $addAccount->bindParam(':friends_count', $accountInfo->friends_count);
      $addAccount->bindParam(':followers_count', $accountInfo->followers_count);
      $addAccount->bindParam(':favourites_count', $accountInfo->favourites_count);
      $addAccount->bindParam(':verified', $accountInfo->verified);
      $addAccount->bindParam(':created_at', $accountInfo->created_at);
      $addAccount->bindParam(':profile_image_url_https', $accountInfo->profile_image_url_https);
      $addAccount->bindParam(':oauthtoken', $accOuth->oauth_token);
      $addAccount->bindParam(':oauthtokensecret', $accOuth->oauth_token_secret);
      $addAccount->execute();

      echo "Account added. |[" . $accountInfo->screen_name . ", " . $accountInfo->followers_count . ']';
      die();
    }
    else {
      echo "Account already added.";
    }

  } else {

    $requestToken = $TwitterOj->oauth_requestToken([
      'oauth_callback' => 'http://' . $_SERVER['HTTP_HOST'] . $_SERVER['REQUEST_URI']
    ]);

    $TwitterOj->setToken($requestToken->oauth_token, $requestToken->oauth_token_secret);

    $addAcc = $TwitterOj->oauth_authorize();
    echo explode("oauth_token=", $addAcc)[1];

  }
