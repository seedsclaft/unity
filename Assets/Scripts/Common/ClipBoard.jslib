//.jslibファイルに張り付けてください。
mergeInto(LibraryManager.library, { 
  CopyWebGL: function(str) {
    var str = Pointer_stringify(str);
    var listener = function(e){
    e.clipboardData.setData("text/plain" , str);    
    e.preventDefault();
    document.removeEventListener("copy", listener);
    }
    document.addEventListener("copy" , listener);
    document.execCommand("copy");
  },
  AsyncPasteWebGL: function() {
      if(navigator.clipboard){
    navigator.clipboard.readText()
    .then(function(text){
        SendMessage('GameSceneManager', 'PasteWebGL', text); //クリップボードから取得したテキストを渡します。引数はゲームオブジェクト名、メソッド名、渡す値
    });
    }
  }
});