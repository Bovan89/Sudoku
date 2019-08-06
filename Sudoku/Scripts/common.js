function Common() {
    _this = this;

    this.init = function () {
        ShowGameTable(3);

        GetSocket('search_games');

        $("#createGame").click(function () {
            //Проверка имени
            var name = $("#user").val();
            if (!CheckName(name))
            {
                return;
            }

            $('#createModal').modal("show");            
        });

        $("body").on("click", "#OKCreate", function (e) {
            var size = $('#selectSize').val();
            var difficult = $('#selectDifficult').val();

            var name = $("#user").val();
            if (!CheckName(name)) {
                return;
            }

            GetSocket('new#' + name + '#' + size + '#' + difficult);

            $('#createModal').modal('hide');
        });

        $("body").on("click", "#CancelCreate", function (e) {
            $('#createModal').modal('hide');
        });

        $("body").on("change", ".intInput", function () {
            var value = $(this).val();
            if (value > size || value < 1) {
                $(this).val('');
                alert('Не корректное число');
            }
            else {
                var s = $(this).attr('id').split('_');
                var x = s[1];
                var y = s[2];
                GetSocket('move#' + playerName + '#' + gameId + '#' + x + '#' + y + '#' + value);
            }
        });

        $("body").on("click", ".game", function () {
            //Вход в чужую игру                        
            var name = $("#user").val();
            if (!CheckName(name)) {
                return;
            }
            GetSocket('join#' + name + '#' + $(this).attr("guid"));
        });

        $("body").on("click", ".exit", function () {            
            GetSocket('exit#' + playerName + '#' + gameId);
            ClearGameArea();            
            GetSocket('search_games');
        });

        $("body").on("click", "#rating", function (e) {
            $('#ratingModal').modal('hide');
        });

        $("body").on("click", "#showRating", function (e) {
            GetSocket('get_top');
        });
    }
}

function GetSocket(par)
{
    if (socket != null && socket.readyState != WebSocket.CLOSED)
    {
        socket.send(par);
        return socket;
    }

    if (typeof (WebSocket) !== 'undefined') {
        //socket = new WebSocket("ws://62.213.76.157:80/WebHandler.ashx");
        socket = new WebSocket("ws://localhost/Sudoku/WebHandler.ashx");
    } else {
        //socket = new MozWebSocket("ws://62.213.76.157:80/WebHandler.ashx");
        socket = new MozWebSocket("ws://localhost/Sudoku/WebHandler.ashx");        
    }

    socket.onopen = function (event) {
        console.log("WebSocket is open now.");
        //socket.send('search_games');
        if (par) {
            socket.send(par);
        }
    };

    socket.onmessage = function (msg) {
        s = msg.data.toString();
        s = s.replace(/\0/g, '');
        var res = s.split("#");

        console.log(s);
        console.log(gameId);
        console.log(playerName);

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
    var i = 0;
    var value = '';
    var id = '';
    var arrValues = str.split(':');
    size = Math.sqrt(arrValues.length);
    
    ClearGameArea();
    ShowGameTable(Math.sqrt(size));

    for (var j = 0; j < arrValues.length; j++) {
        value = arrValues[j];
        id = i + "_" + (j % size);

        $("#cell_" + id).text('');

        if (value != 0) {
            $("#cell_" + id).text(value);
        }
        else {         
            $("#cell_" + id).html('<input type="number" class="intInput" id="t_' + id + '" min="1" max="9" />');
        }        
                
        if ((j + 1) % size == 0) {
            i++;
        }
    }        

    var html = '<button type="button" class="btn btn-primary btn-lg btn-block exit">Выйти</button>';
    $("#info").html(html);
}

function ClearGameArea()
{
    gameId = '';
    playerName = '';

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

function ShowGameTable(n) {
    var html = '';
    var n2 = n * n;
    var id = '', cell_class = '';

    for (var i = 0; i < n2; i++) {
        html = html + '<tr>';

        for (var j = 0; j < n2; j++) {            
            cell_class = '';
            id = '_' + i + '_' + j;
            
            if ((i + 1) % n == 1) {
                //top
                cell_class = cell_class + ' td_top';
            }
            if ((i + 1) % n == 0) {
                //bottom
                cell_class = cell_class + ' td_bottom';
            }
            if ((j + 1) % n == 1) {
                //left
                cell_class = cell_class + ' td_left';
            }
            if ((j + 1) % n == 0) {
                //right
                cell_class = cell_class + ' td_right';
            }
            if (cell_class != '')
            {
                cell_class = 'class="' + cell_class.substr(1) + '"';
            }

            html = html + '<td id="cell' + id + '"' + cell_class + '></td>';
        }

        html = html + '</tr>' + '\n';
    }

    $("#gameTable").html(html);
}

var common = null;

var gameId = '';
var playerName = '';
var socket = null;
var size = 9;

$().ready(function () {
    common = new Common();
    common.init();
});