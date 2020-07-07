var selected = null;
var whitesTurn = true;
var bSq = 'black-square';
var wSq = 'white-square';
var wB = 'white-bob';
var bB = 'black-bob';
var sB = 'selected-bob';
function generateSquare(isWhite) {
    var node = document.createElement('div');
    var appriopriateClass = isWhite ? 'white-square' : 'black-square';
    node.classList.add(appriopriateClass);
    return node;
}
function generateBob(isWhite) {
    var node = document.createElement('div');
    var appClass = isWhite ? 'white-bob' : 'black-bob';
    node.classList.add(appClass);
    return node;
}
function generateSelected() {
    var node = document.createElement('div');
    node.classList.add(sB);
    return node;
}

function hasBob(x, y, white) {
    var sq = $('#row' + y).children('#col' + x);
    var cn = white ? wB : bB;
    return sq.children().first().hasClass(cn);
}

function isEmpty(x, y) {
    var el = $('#row' + y).children('#col' + x).children().first();
    return !(el.hasClass(wB) || el.hasClass(bB));
}

function isValidMove(fromx, fromy, tox, toy, isWhite) {
    if (isWhite) {
        return (+fromx + 1 == +tox && +fromy + 1 == +toy) || (+fromx - 1 == +tox && +fromy + 1 == +toy);
    } else {
        return (+fromx + 1 == +tox && +fromy - 1 == +toy) || (+fromx - 1 == +tox && +fromy - 1 == +toy);
    }
}

// square, square
function handleMovement(from, to) {
    if (to.children().hasClass(wB) || to.children().hasClass(bB)) {
        return false;
    }
    var fromY = from.parent().attr('id')[3];
    var fromX = from.attr('id')[3];
    var toY = to.parent().attr('id')[3];
    var toX = to.attr('id')[3];
    var isWhite = from.children().hasClass(wB);
    if (isValidMove(fromX, fromY, toX, toY, isWhite)) {
        to.append(generateBob(isWhite));
        from.empty();
        return true;
    } else {
        return false;
    }
}

function parseSquare(sq) {
    var x = parseInt(sq.attr('id')[3]);
    var y = parseInt(sq.parent().attr('id')[3]);
    return { x, y };
}

function thereIsABeating() {
    var cn = whitesTurn ? wB : bB;
    var bt = false;
    $('.' + cn).parent().each(function () {
        var result = sqHasBeating($(this));
        if (result) {
            bt = true;
            return false;
        }
        return true;
    });
    return bt;
}

function beats(from, to) {
    var coord = { x: (from.x + to.x) / 2, y: (from.y + to.y) / 2 };
    var midSquare = $('#row' + coord.y).children('#col' + coord.x);
    if (midSquare.length == 0) return;
    if (whitesTurn) {
        if (midSquare.children().first().hasClass(bB)) {
            midSquare.empty();
            return true;
        }
    } else {
        if (midSquare.children().first().hasClass(wB)) {
            midSquare.empty();
            return true;
        }
    }
    return false;
}

function selectible(square) {
    if (square.children().hasClass(wB) && whitesTurn) {
        return true;
    } else
        if (square.children().hasClass(bB) && !whitesTurn) {
            return true;
        }
    return false;
}

function sqHasBeating(sq) {
    var b = parseSquare(sq);
    var isWhite = sq.children().hasClass(wB);
    return (hasBob(b.x + 1, b.y + 1, !isWhite) && isEmpty(b.x + 2, b.y + 2)) ||
        (hasBob(b.x + 1, b.y - 1, !isWhite) && isEmpty(b.x + 2, b.y - 2)) ||
        (hasBob(b.x - 1, b.y + 1, !isWhite) && isEmpty(b.x - 2, b.y + 2)) ||
        (hasBob(b.x - 1, b.y - 1, !isWhite) && isEmpty(b.x - 2, b.y - 2));
}

var selectHasBeating = null;
function handleClick() {
    var caller = $(this);
    if (selectible(caller)) {
        $('.' + sB).remove();
        caller.children().append(generateSelected());

    } else {
        var selected = $('.' + sB).parent().parent();
        if (selected != null) {
            if (thereIsABeating()) {
                if (beats(parseSquare(selected), parseSquare(caller))) {
                    selected.empty();
                    caller.append(generateBob(whitesTurn));
                    if (sqHasBeating(caller)) {
                        return;
                    } else {
                        whitesTurn = !whitesTurn;
                    }
                }
            } else {
                if (handleMovement(selected, caller)) {
                    whitesTurn = !whitesTurn;
                }
            }
        }
    }
}

var isWhite = true;
function generateBoard() {
    for (var i = 0; i < 10; ++i) {
        var row = document.createElement('div');
        row.id = 'row' + i;
        for (var j = 0; j < 10; ++j) {
            var sq = generateSquare(isWhite);
            sq.id = 'col' + j;
            row.appendChild(sq);
            isWhite = !isWhite;
        }
        isWhite = !isWhite;
        $('#board').append(row);
    }

    for (var i = 0; i < 3; ++i) {
        $('#row' + i).children('.black-square').append(generateBob(true));
        $('#row' + (9 - i)).children('.black-square').append(generateBob(false));
    }

    $('.' + bSq).click(handleClick);
}
$(document).ready(() => generateBoard());