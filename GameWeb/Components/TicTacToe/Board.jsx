import React from 'react';
import styled from 'react-emotion';

import Square from './Square';

const BoardRow = styled('div')`
    &:after {
        clear: both;
        content: "";
        display: table;
    }
`;

class Board extends React.Component {
    renderSquare(i) {
        return (
            <Square 
                value={this.props.getSquareState(i)} 
                onClick={() => this.props.onClick(i)}
            />
        );
    }

    render() {
        return (
            <div>
                <BoardRow>
                    {this.renderSquare(0)}
                    {this.renderSquare(1)}
                    {this.renderSquare(2)}
                </BoardRow>
                <BoardRow>
                    {this.renderSquare(3)}
                    {this.renderSquare(4)}
                    {this.renderSquare(5)}
                </BoardRow>
                <BoardRow>
                    {this.renderSquare(6)}
                    {this.renderSquare(7)}
                    {this.renderSquare(8)}
                </BoardRow>
            </div>
        );
    }
}

export default Board;