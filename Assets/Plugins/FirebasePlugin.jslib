mergeInto(LibraryManager.library, {

  FirebaseInit: function () {
    const FirebaseConfig = {
        apiKey: "AIzaSyBezj2DIhbIHqnJ1Z3Lun9dVf3iKX9_l5M",
        authDomain: "numinos-9a795.firebaseapp.com",
        projectId: "numinos-9a795",
        storageBucket: "numinos-9a795.appspot.com",
        messagingSenderId: "411399856788",
        appId: "1:411399856788:web:05bf5f4f329b0185200a81",
        measurementId: "G-FQ7NQ9K52L"
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
                res += ";";
            }
            idx++;
        });
        var bufferSize = lengthBytesUTF8(res) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(res, buffer, bufferSize);
        Module.dynCall_vii(callback,instanceId,buffer);
    })
    .catch(function(error) {
        console.log("Error getting documents: ", error);
    });     
  },


  FirebaseCurrentRankingData: function (instanceId,userId,callback) {
    console.log("FirebaseCurrentRankingData");
    const db = window.firebase.firestore();
    const docRef = db.collection("ranking").doc(userId);
    
    let res = "-1";
    docRef.get().then((doc) => {
        if (doc.exists) {
            res = doc.data()["Score"];
        }
        var bufferSize = lengthBytesUTF8(res) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(res, buffer, bufferSize);
        Module.dynCall_vii(callback,instanceId,buffer);
    }).catch((error) => {
        console.log("Error getting document:", error);
    });
  },

  FirebaseWriteRankingData: function (instanceId,userId,score,name,selectIndex,selectRank,callback) {
    console.log("FirebaseWriteRankingData");
    const db = window.firebase.firestore();
    let res = "";
    db.collection("ranking").doc(userId).set({
        Name: name,
        Score: score,
        SelectIdx: selectIndex,
        SelectRank: selectRank
    })
    .then(() => {
        console.log("Document successfully written!");
        var bufferSize = lengthBytesUTF8(res) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(res, buffer, bufferSize);
        Module.dynCall_vii(callback,instanceId,buffer);
    })
    .catch((error) => {
        console.error("Error writing document: ", error);
    });
  },

  PrintFloatArray: function (array, size) {
    for(var i = 0; i < size; i++)
    console.log(HEAPF32[(array >> 2) + i]);
  },

  AddNumbers: function (x, y) {
    return x + y;
  },

  StringReturnValueFunction: function () {
    var returnStr = "bla";
    var bufferSize = lengthBytesUTF8(returnStr) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize);
    return buffer;
  },

  BindWebGLTexture: function (texture) {
    GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[texture]);
  },

});