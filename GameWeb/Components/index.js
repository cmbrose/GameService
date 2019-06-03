import React from 'react';
import ReactDOM from 'react-dom';
import ReactDOMServer from 'react-dom/server';

import TicTacToe from './TicTacToe/index.js';
import GameLink from './GameLink.jsx';

import { renderStylesToString } from 'emotion-server';

global.React = React;
global.ReactDOM = ReactDOM;
global.ReactDOMServer = ReactDOMServer;

global.EmotionServer = { renderStylesToString };

global.GameLink = GameLink;

global.TicTacToe = TicTacToe;