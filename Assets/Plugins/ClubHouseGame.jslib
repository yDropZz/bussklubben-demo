mergeInto(LibraryManager.library, {
  ExtGameDone: function () {
    if (window.ClubHouseGame) {
      window.ClubHouseGame.gameDone();
    } else {
      console.log("ClubHouseGame not loaded");
    }
  },

  ExtSetScore: function (score) {
    if (window.ClubHouseGame) {
      window.ClubHouseGame.setScore(score);
    } else {
      console.log("ClubHouseGame not loaded");
    }
  },

  ExtGetScore: function () {
    if (window.ClubHouseGame) {
      return window.ClubHouseGame.getScore();
    } else {
      console.log("ClubHouseGame not loaded");
    }
  },

  ExtGameRunning: function () {
    if (window.ClubHouseGame) {
      return window.ClubHouseGame.gameRunning();
    } else {
      console.log("ClubHouseGame not loaded");
    }
  },

  ExtRegisterStartMethod: function (script, method) {
    if (window.ClubHouseGame) {
      var scriptString = UTF8ToString(script);
      var methodString = UTF8ToString(method);

      var restart = function () {
        console.log("sending message to " + scriptString + " " + methodString);
        unityInstance.SendMessage(scriptString, methodString);
      };

      window.ClubHouseGame.registerRestart(restart);
      window.ClubHouseGame.gameLoaded({ hideInGame: true });
    } else {
      console.log("ClubHouseGame not loaded");
    }
  },
});
