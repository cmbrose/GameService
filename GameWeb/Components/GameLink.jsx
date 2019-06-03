import React from 'react';
import styled from 'react-emotion';

const LinkContainer = styled('a')`
    padding: 6px;
    margin-bottom: 4px;
    border: 2px solid black;
    border-radius: 3px;
    width: 300px;
    display: block;

    background-color: lightgrey;

    &:hover {
        background-color: grey;
    }
`;

class GameLink extends React.Component {
    constructor(props) {
        super(props);
        this.state = { data: this.props.initialData };
    }

    render() {
        return (
            <LinkContainer href={"Games/" + this.props.id}>
                {this.props.id}
            </LinkContainer>
        );
    }
}

export default GameLink;