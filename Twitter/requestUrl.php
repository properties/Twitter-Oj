hp<?php

  require_once ('Corecode.php');
  ini_set("display_errors", 1);

  define('KEY', 'X');
  define('KEYSECRET', 'X');

  define('DB_HOST', 'localhost');
  define('DB_USER', 'X');
  define('DB_PASS', 'X');
  define('DB_DATABASE', 'X');
  define('DB_TABLE', 'twitter');

  \Corecode\Corecode::setConsumerKey(KEY, KEYSECRET);
  $Corecode = \Corecode\Corecode::getInstance();


  if (isset($_GET['oauth_verifier'])) {

    $Corecode->setToken($_GET['oauth_token'], $_GET['oauth_token_secret']);

    $accOuth = $Corecode->oauth_accessToken([
      'oauth_verifier' => $_GET['oauth_verifier']
    ]);

    $databaseConnection = mysqli_connect(DB_HOST,DB_USER,DB_PASS,DB_DATABASE);
    $saveOuth = mysqli_query($databaseConnection, "INSERT INTO twitter (oauthtoken, oauthtokensecret, login) VALUES ('".$accOuth->oauth_token."', '".$accOuth->oauth_token_secret."', '".$_GET['login']."')");

    echo "Account added.";
    die();

  } else {

    $requestToken = $Corecode->oauth_requestToken([
      'oauth_callback' => 'http://' . $_SERVER['HTTP_HOST'] . $_SERVER['REQUEST_URI']
    ]);

    $Corecode->setToken($requestToken->oauth_token, $requestToken->oauth_token_secret);

    $addAcc = $Corecode->oauth_authorize();
    echo explode("oauth_token=", $addAcc)[1];

  }
