var FirebasePlugin = {

  InitializeWebGL: function () {
    console.log("InitializeWebGL");
    const FirebaseConfig = {
      apiKey: "AIzaSyCWY2ryuuifGEqqgDaktajcYYFFsWToI0A",
      authDomain: "norm-4d161.firebaseapp.com",
      projectId: "norm-4d161",
      storageBucket: "norm-4d161.appspot.com",
      messagingSenderId: "426265705279",
      appId: "1:426265705279:web:819d8d95887bc24ae8718a",
      measurementId: "G-JWHMEGC4VZ"
    };
    window.firebase.initializeApp(FirebaseConfig);
  },

  FirebaseReadRankingData: function (instanceId,callback) {
    console.log("FirebaseReadRankingData");
    const db = window.firebase.firestore();
    const cnf = db.collection("ranking");
    let res = "";
    let idx = 0;
    cnf.orderBy("Score","desc").limit(100).get().then(function(q) {
        q.docs.forEach((doc) => {
            var json = JSON.stringify(doc.data());
            res += json;
            if (idx < q.docs.length-1){
                res += ";.:";
            }
            idx++;
        });
        Module.dynCall_vii(callback,instanceId,utils.StringReturnValueFunction(res));
    })
    .catch(function(error) {
        console.log("Error getting documents: ", error);
        Module.dynCall_vii(callback,instanceId,utils.StringReturnValueFunction(res));
    });     
  },

  FirebaseCurrentRankingData: function (instanceId,userId,callback) {
    console.log("FirebaseCurrentRankingData");
    const db = window.firebase.firestore();
    const userName = UTF8ToString(userId.toString());
    const docRef = db.collection("ranking").doc(userName);
    
    let res = "-1";
    docRef.get().then((doc) => {
        if (doc.exists) {
          var json = JSON.stringify(doc.data());
          res = json;
        }
        Module.dynCall_vii(callback,instanceId,utils.StringReturnValueFunction(res));
    }).catch((error) => {
        console.log("Error getting document:", error);
        Module.dynCall_vii(callback,instanceId,utils.StringReturnValueFunction(res));
    });
  },

  FirebaseWriteRankingData: function (instanceId,userId,score,name,selectIndex,selectIndexSize,selectRank,selectRankSize,callback) {
    console.log("FirebaseWriteRankingData");
    const db = window.firebase.firestore();
    const userName = UTF8ToString(userId.toString());
    let res = "";

    var startIndex = selectIndex / HEAP32.BYTES_PER_ELEMENT;
    var selectIndexData = HEAP32.subarray(startIndex, startIndex + selectIndexSize);
    var sendIndex = [];
    selectIndexData.forEach((data) => {
      sendIndex.push(data);
    });

    var startIndex2 = selectRank / HEAP32.BYTES_PER_ELEMENT;
    var selectRankData = HEAP32.subarray(startIndex2, startIndex2 + selectRankSize);
    var sendRank = [];
    selectRankData.forEach((data) => {
      sendRank.push(data);
    });

    db.collection("ranking").doc(userName).set({
        Name: UTF8ToString(name),
        Score: score,
        SelectIdx: sendIndex,
        SelectRank: sendRank
    })
    .then(() => {
        console.log("Document successfully written!");
        Module.dynCall_vii(callback,instanceId,utils.StringReturnValueFunction(res));
    })
    .catch((error) => {
        console.error("Error writing document: ", error);
        Module.dynCall_vii(callback,instanceId,utils.StringReturnValueFunction(res));
    });
  },

};

var lib2 = {
    $utils: {
      StringReturnValueFunction: function (returnStr) {
        var bufferSize = lengthBytesUTF8(returnStr) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(returnStr, buffer, bufferSize);
        return buffer;
      },
    }
};

autoAddDeps(FirebasePlugin, '$utils');
mergeInto(LibraryManager.library, lib2);
mergeInto(LibraryManager.library, FirebasePlugin);