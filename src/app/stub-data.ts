import { Player } from './models/player';

export const players: Player[] = [
  {
    id: '1',
    alias: 'P1',
    name: 'Player One',
    facebookId: 'p1',
    whatsAppNumber: null,
    preferences: {
      avoid: [],
      prefer: [],
    },
  },
  {
    id: '2',
    alias: 'Cool Stuff',
    name: 'Player Two',
    facebookId: '123456',
    whatsAppNumber: null,
    preferences: {
      avoid: ['3', '4'],
      prefer: [],
    },
  },
  {
    id: '3',
    alias: 'Wow!!!',
    name: 'Player Three',
    facebookId: 'player.three',
    whatsAppNumber: null,
    preferences: {
      avoid: [],
      prefer: [],
    },
  },
  {
    id: '4',
    alias: null,
    name: 'Player Four',
    facebookId: 'kgljdfhlgkjsh',
    whatsAppNumber: null,
    preferences: {
      avoid: ['5'],
      prefer: [],
    },
  },
  {
    id: '5',
    alias: null,
    name: 'Player Five',
    facebookId: 'fljksdh',
    whatsAppNumber: null,
    preferences: {
      avoid: [],
      prefer: [],
    },
  },
  {
    id: '6',
    alias: 'Player 6',
    name: 'Player Six',
    facebookId: null,
    whatsAppNumber: '123456789',
    preferences: {
      avoid: [],
      prefer: [],
    },
  },
  {
    id: '7',
    alias: 'Invalid Player',
    name: 'Player Seven',
    facebookId: null,
    whatsAppNumber: null,
    preferences: {
      avoid: [],
      prefer: [],
    },
  },
];
