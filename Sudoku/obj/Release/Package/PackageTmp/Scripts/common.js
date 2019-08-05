function Common() {
    _this = this;

    this.init = function () {
        /*var socket;        

        if (typeof (WebSocket) !== 'undefined') {
            socket = new WebSocket("ws://localhost/Sudoku/WebHandler.ashx");
        } else {
            socket = new MozWebSocket("ws://localhost/Sudoku/WebHandler.ashx");
        }*/
        
        /*socket.onclose = function (event) {
            alert('Мы потеряли её. Пожалуйста, обновите страницу');
        };*/

        GetSocket();

        $("#createGame").click(function () {
            //Проверка имени
            playerName = $("#user").val();
            if (!CheckName(playerName))
            {
                return;
            }           
            GetSocket().send('new#' + playerName);
        });

        $("body").on("change", ".intInput", function () {
            var value = $(this).val();
            if (value > 9 || value < 1) {
                $(this).val('');
                alert('Не корректное число');
            }
            else {
                var s = $(this).attr('id').split('_');
                var x = s[1];
                var y = s[2];
                GetSocket().send('move#' + playerName + '#' + gameId + '#' + x + '#' + y + '#' + value);
            }
        });

        $("body").on("click", ".game", function () {
            //Вход в чужую игру                        
            playerName = $("#user").val();
            if (!CheckName(playerName)) {
                return;
            }
            GetSocket().send('join#' + playerName + '#' + $(this).attr("guid"));
        });

        $("body").on("click", ".exit", function () {            
            GetSocket().send('exit#' + playerName + '#' + gameId);
            ClearGameArea();            
            GetSocket().send('search_games');
        });

        $("body").on("click", "#rating", function (e) {
            $('#ratingModal').modal('hide');
        });

        $("body").on("click", "#showRating", function (e) {
            GetSocket().send('get_top');            
        });
    }
}

function GetSocket()
{
    if (socket != null && socket.readyState != WebSocket.CLOSED)
    {
        return socket;
    }

    if (typeof (WebSocket) !== 'undefined') {
        socket = new WebSocket("ws://localhost/Sudoku/WebHandler.ashx");
    } else {
        socket = new MozWebSocket("ws://localhost/Sudoku/WebHandler.ashx");
    }

    socket.onopen = function (event) {
        console.log("WebSocket is open now.");
        socket.send('search_games');
    };

    socket.onmessage = function (msg) {
        s = msg.data.toString();
        s = s.replace(/\0/g, '');
        var res = s.split("#");

        switch (res[0]) {
            case 'get_top':
                ShowTop(res);
                break;
            case 'win':
            case 'exit_player':
            case 'new_player':
                if (gameId != '' && gameId != null) {
                    AddToLog(res);
                }
                break;
            case 'move':
                if (gameId != '' && gameId != null) {
                    AddToLog(res);
                    FillCell(res[2], res[3], res[4]);
                }
                break;
            case 'error':
                alert(res[1]);
                break;
            case 'error_move':
                ClearCell(res[1], res[2]);
                alert("Невозможно!");
                break;
            case 'search_games':
                console.log(s);
                ShowGames(res);
                break;
            case 'join':
            case 'new':
                FillGameArea(res[2]);
                gameId = res[1];
                playerName = res[3];
                break;
            default:
                return;
        }
    };

    return socket;
}

function CheckName(name) {
    if (name.trim() == '' || name.search('#') >= 0) {
        alert("Некорректное имя");
        return false;
    }

    return true;
}

function ShowGames(res) {    
    var html = '';

    for (var i = 1; i < res.length - 1; i += 2) {        
        var html = html + '\n' + '<button type="button" class="btn btn-primary btn-lg btn-block game" guid="' + res[i + 1] + '">' + res[i] + '</button>';
    }

    $("#info").html(html);
}

function FillGameArea(str)
{
    //Заполнение полей игры
    var l = Math.sqrt(str.length);
    var i = 0;
    var value = '';
    var id = '';

    ClearGameArea();    

    for (var j = 0; j < str.length; j++) {
        value = str.charAt(j);
        id = i + "_" + (j % l);

        $("#cell_" + id).text('');

        if (value != 0) {
            $("#cell_" + id).text(value);
        }
        else {         
            $("#cell_" + id).html('<input type="number" class="intInput" id="t_' + id + '" min="1" max="9" />');
        }        
                
        if ((j + 1) % l == 0) {
            i++;
        }
    }        

    var html = '<button type="button" class="btn btn-primary btn-lg btn-block exit">Выйти</button>';
    $("#info").html(html);
}

function ClearGameArea()
{
    $("#gameArea").attr("gameId", "");
    $("#gameArea").attr("playerName", "");   
    $(".intInput").remove();
    $("td").text('');
    $(".exit").remove();
    $("#info").html('');
    $("#log").html('');
}

function ClearCell(x, y) {
    $("#t_" + x + "_" + y).val("");
}

function AddToLog(res) {
    var html = $("#log").html();
    var info = "";

    for (var i = 0; i < res.length - 1; i++) {
        info = info + " " + res[i];
    }

    html = html + '<p>' + res[res.length - 1] + ':' + info + '</p>';
    $("#log").html(html);
}

function FillCell(x, y, value)
{
    $("#t_" + x + "_" + y).remove();
    $("#cell_" + x + "_" + y).text(value);
}

function ShowTop(res) {
    var html = "";

    for (var i = 1; i < res.length - 1; i++) {
        html = html + '<p>' + res[i] + '</p>';
    }

    $("#ratingInfo").html(html);
    $('#ratingModal').modal("show");
}

var common = null;

var gameId = '';
var playerName = '';
var socket = null;

$().ready(function () {
    common = new Common();
    common.init();
});