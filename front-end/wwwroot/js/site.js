let connection;
const headerMiddle = document.getElementById('header-middle');
const playButton = document.getElementById("play");
const backButton = document.getElementById('back-button');
const finishedTitle = document.getElementById('finished');
const finishedOverlay = document.getElementById('overlay');

const buttonState = {
    play: 0,
    start: 1
};

const playerElem = {
    p1: document.getElementById('p1'),
    p2: document.getElementById('p2')
};

const state = {
    button: buttonState.play,
    gameState: 0,
    turn: 0,
    players: {},
    playerId: null,
    pieces: {},
    isSecondClick: false,
    isThirdClick: false,
    lastClicked: null
};


const colors = {
    0: 'piece-red',
    1: 'piece-black'
};


const icons = {
    0: document.getElementById('king'),
    1: document.getElementById('adviser'),
    2: document.getElementById('bishop'),
    3: document.getElementById('rook'),
    4: document.getElementById('cannon'),
    5: document.getElementById('knight'),
    6: document.getElementById('pawn')
};

playButton.onclick = handleButton;

async function handleButton() {
    switch (state.button) {
        case buttonState.start:
            start();
            break;
        default:
            await play();
            break;
    }
}

function start() {
    playButton.disabled = true;
    playButton.innerText = 'Waiting for opponnent...';
    connection.invoke("Ready").catch(err => console.error(err.toString()));
}

async function play() {
    try {
        playButton.innerText = "Connecting...";
        playButton.disabled = true;
        const response = await fetch("api/play");
        const data = await response.text();
        console.log(data);

        connection = new signalR
            .HubConnectionBuilder()
            .withUrl(data)
            .build();

        initialize(connection);
        await connect();
    } catch (err) {
        console.error(err.toString());
    }
}

async function connect() {
    try {
        await connection.start();
        state.playerId = connection.connection.connectionId;
    } catch (err) {
        console.error(err.toString());
    }
}

function getDirections(position) {
    const up = position - 8;
    const down = position + 8;
    const left = position - 1;
    const right = position + 1;

    return [up, right, down, left];
}

const elems = {};


let x = 1;
document.querySelectorAll('.piece').forEach(p => {
    const i = x;
    const pieces = state.pieces;

    pieces[i] = {
        isFlipped: false,
        color: null,
        type: null
    };

    const iconHolder = p.querySelector('.piece-back');
    const container = p.parentNode;

    elems[i] = {
        pieceHolder: p,
        iconHolder: iconHolder,
        container: container
    };

    p.onclick = (e) => {
        const piece = pieces[i];
        e.stopPropagation();
        if (state.gameState !== 1) return;
        console.log('in1');
        const color = state.players[state.playerId].color;

        if (state.turn != color || state.isThirdClick) return;
        console.log('in2');

        if (state.isSecondClick) {
            console.log('in3');
            if (!state.lastClicked) return;
            console.log('in4');

            if (piece) {
                if (!piece.isFlipped) return;
                console.log('in5');

                if (piece.color === color) return;
                console.log('in6');

                if (!getDirections(i).includes(state.lastClicked)) return;
                console.log('in6.5');

                if (pieces[state.lastClicked].type > piece.type)
                    if (piece.type !== 0 && pieces[state.lastClicked].type !== 6)
                        return;
            }
            console.log('in7');

            state.isThirdClick = true;
        } else {
            console.log('in8');
            if (piece.isFlipped) {
                console.log('in9');
                if (piece.color !== color) return;
                console.log('in10');
                container.classList.add('piece-selected');
                state.isSecondClick = true;
                state.lastClicked = i;

                document.querySelector('.game-overlay').onclick = (e) => {
                    e.stopPropagation();
                    container.classList.remove('piece-selected');
                    state.isSecondClick = false;
                    state.isThirdClick = false;
                    state.lastClicked = null;
                };
                return;
            }
        }

        const action = !piece || piece.isFlipped ? 1 : 0;
        const from = action == 0 ? i : state.lastClicked;
        const to = action == 1 ? i : 0;
        connection.invoke("Action", { action, from, to })
            .then(response => {
                if (response) {
                    if (action == 0) {
                        console.log('in11');
                        piece.isFlipped = true;
                        piece.color = response.color;
                        piece.type = response.type;

                        const icon = icons[response.type].cloneNode();
                        icon.style.display = 'initial';

                        iconHolder.append(icon);
                        p.classList.add('piece-flipped');
                        p.classList.add(colors[color]);
                    } else {
                        console.log('in12');
                        const fromElem = elems[from];
                        fromElem.pieceHolder.style.opacity = 0;
                        fromElem.pieceHolder.classList.remove(colors[color]);
                        fromElem.container.classList.remove('piece-selected');
                        fromElem.iconHolder.removeChild(fromElem.iconHolder.firstChild);

                        state.isSecondClick = false;
                        state.isThirdClick = false;
                        state.lastClicked = null;

                        const fromPiece = pieces[from];

                        const icon = icons[fromPiece.type].cloneNode();
                        icon.style.display = 'initial';

                        p.style.opacity = 1;
                        if (piece) {
                            p.classList.remove(colors[piece.color]);
                            iconHolder.replaceChild(icon, iconHolder.firstChild);
                        } else {
                            iconHolder.append(icon);
                        }
                        p.classList.add(colors[color]);

                        pieces[i] = fromPiece;
                        pieces[from] = null;
                    }
                    state.turn = response.turn;
                    headerMiddle.innerHTML = getTurn();
                    console.log(state);
                } else console.log('action response is null');
            }).catch(err => console.log(err));
    };
    x++;
});

function initialize(connection) {
    connection.on("Connected", players => {
        state.players = players;
        Object.entries(players).forEach(([id, player]) => {
            if (id == state.playerId) {
                playerElem.p1.querySelector('.player-img').classList.add(colors[player.color]);
            } else {
                playerElem.p2.querySelector('.player-img').classList.add(colors[player.color]);
                playerElem.p2.style.opacity = 1;
            }
        });
        state.button = buttonState.start;

        playButton.innerText = "Start";
        playButton.disabled = false;
    });

    connection.on("Join", dto => {
        state.players[dto.id] = dto.player;
        playerElem.p2.querySelector('.player-img').classList.add(colors[dto.player.color]);
        playerElem.p2.style.opacity = 1;
    });

    connection.on("GameStarted", () => {
        state.gameState = 1;
        console.log("game has started");

        headerMiddle.innerHTML = getTurn();
    });

    connection.on('Action', dto => {
        if (dto.action == 0) {
            console.log('in1');

            const piece = state.pieces[dto.from];
            const elem = elems[dto.from];

            piece.isFlipped = true;
            piece.color = dto.color;
            piece.type = dto.type;
            console.log(state);

            const icon = icons[dto.type].cloneNode();
            icon.style.display = 'initial';
            elem.iconHolder.append(icon);

            elem.pieceHolder.classList.add('piece-flipped');
            elem.pieceHolder.classList.add(colors[dto.color]);
        } else {
            console.log('in2');
            const piece = state.pieces[dto.to];
            const elem = elems[dto.to];
            const fromPiece = state.pieces[dto.from];
            const fromElem = elems[dto.from];

            fromElem.pieceHolder.style.opacity = 0;
            fromElem.pieceHolder.classList.remove(colors[fromPiece.color]);
            fromElem.iconHolder.removeChild(fromElem.iconHolder.firstChild);

            const icon = icons[fromPiece.type].cloneNode();
            icon.style.display = 'initial';

            elem.pieceHolder.style.opacity = 1;
            elem.pieceHolder.classList.add(colors[fromPiece.color]);
            if (piece) {
                elem.pieceHolder.classList.remove(colors[piece.color]);
                elem.iconHolder.replaceChild(icon, elem.iconHolder.firstChild);
            } else {
                elem.iconHolder.append(icon);
            }

            state.pieces[dto.to] = fromPiece;
            state.pieces[dto.from] = null;
        }
        state.turn = dto.turn;
        headerMiddle.innerHTML = getTurn();
        console.log(state);
    });

    connection.on("GameFinished", () => {
        if (state.turn == state.players[state.playerId].color) {
            finishedTitle.innerText = "You Won";
            finishedTitle.style.color = 'lightgreen';
        } else {
            finishedTitle.innerText = "You Lost";
            finishedTitle.style.color = 'tomato';
        }
        finishedOverlay.style.display = 'flex';
    });
}

backButton.onclick = () => location.reload();

function getTurn() {
    return state.turn == state.players[state.playerId].color
        ? "Your Turn" : "Opponnent Turn";
}