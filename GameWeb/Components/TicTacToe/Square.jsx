import React from 'react';
import styled from 'react-emotion';

const SquareButton = styled('button')`
    border: 1px solid black;
    width: 50px;
    line-height: 50px;
    height: 50px;
    float: left;
    background-color: white;
    margin-right: -1px;
    margin-top: -1px;
    padding: 0;

    &:hover {
        background-color: lightgrey;
    }
`;

class Square extends React.Component {
    constructor(props) {
        super(props);
        this.state = { data: this.props.initialData };
    }

    render() {
        return (
          <SquareButton onClick={this.props.onClick}>
              {this.props.value}
          </SquareButton>
        );
    }
}

export default Square;