import React from 'react';
import styled from 'react-emotion';

import Board from './Board';

const GameContainer = styled('div')`
    display: flex;
    flex-direction: row;
`;

class Game extends React.Component {
    constructor(props) {
        super(props);
        this.state = { 
            squares: this.props.initialData,
            xIsNext: true,
        };
    }

    componentDidMount() {
        window.setInterval(
            () => this.pollForStateUpdate(),
            this.props.pollInterval | 2000,
        );
    }

    handleClick(i) {
        if (!this.props.playable) {
            return;
        }

        var self = this;

        $.ajax({
            url: this.props.rootCallbackUrl,
            type: 'PUT',
            data: {
                player: this.state.xIsNext ? 'X' : 'O',
                idx: i
            },
            dataType: 'json',
            success: function (newSquares) {
                self.setState({
                    squares: newSquares,
                    xIsNext: !self.state.xIsNext,
                });
            },
            error: function (err) {
                console.log(err);
            }
        });        
    }

    pollForStateUpdate() {
        var self = this;

        $.ajax({
            url: this.props.rootCallbackUrl + '/GetState',
            type: 'GET',
            dataType: 'json',
            success: function (newSquares) {
                self.setState({
                    squares: newSquares,
                });
            },
            error: function (err) {
                console.log(err);
            }
        }); 
    }

    getSquareState(i) {
        return this.state.squares[i];
    }

    render() {
        const wrapperStyle = {
            "pointer-events": this.props.playable ? "unset" : "none"
        };

        return (
            <GameContainer style={wrapperStyle}>
                <Board 
                    getSquareState={(i) => this.getSquareState(i)}
                    onClick={(i) => this.handleClick(i)}
                />
            </GameContainer>
        );
    }
}

export default Game;