import { Draft } from 'immer';

import { MinigolfEvent } from './models/event';
import { MinigolfMap } from './models/minigolf-map';
import { User } from './models/user';
import { createTime } from './utils/date.utils';

let nextId = 1;
export function getId() {
  return `${nextId++}`;
}

// #region Users
export const users: Draft<User>[] = [
  {
    id: getId(),
    isAdmin: true,
    loginType: 'facebook',
    name: 'Admin User',
  },
  {
    id: getId(),
    isAdmin: false,
    loginType: 'facebook',
    name: 'Normal User',
  },
  {
    id: getId(),
    isAdmin: false,
    loginType: 'email',
    name: 'Email User',
  },
  {
    id: getId(),
    isAdmin: false,
    loginType: 'email',
    name: 'Another Email User',
  },
  {
    id: getId(),
    isAdmin: false,
    loginType: 'email',
    name: 'Yet Another Email User',
  },
  {
    id: getId(),
    isAdmin: false,
    loginType: 'email',
    name: 'Last Email User',
  },
];
// #endregion

// #region Maps
export const maps: Draft<MinigolfMap>[] = [
  { id: getId(), name: 'Tourist Trap leicht' },
  { id: getId(), name: 'Tourist Trap hart' },
  { id: getId(), name: 'Cherry Blossom leicht' },
  { id: getId(), name: 'Cherry Blossom hart' },
  { id: getId(), name: 'Seagull Stacks leicht' },
  { id: getId(), name: 'Seagull Stacks hart' },
  { id: getId(), name: 'Arizona Modern leicht' },
  { id: getId(), name: 'Arizona Modern hart' },
  { id: getId(), name: 'Original Gothic leicht' },
  { id: getId(), name: 'Original Gothic hart' },
  { id: getId(), name: 'Bogeys Bonanza leicht' },
  { id: getId(), name: 'Bogeys Bonanza hart' },
  { id: getId(), name: 'Tethys Station leicht' },
  { id: getId(), name: 'Tethys Station hart' },
  { id: getId(), name: 'Quixote Valley leicht' },
  { id: getId(), name: 'Quixote Valley hart' },
  { id: getId(), name: 'Sweetopia leicht' },
  { id: getId(), name: 'Sweetopia hart' },
  { id: getId(), name: 'Upside Town leicht' },
  { id: getId(), name: 'Upside Town hart' },
  { id: getId(), name: 'Labyrinth leicht' },
  { id: getId(), name: 'Labyrinth hart' },
  { id: getId(), name: 'Myst leicht' },
  { id: getId(), name: 'Myst hart' },
  { id: getId(), name: 'Laser Lair leicht' },
  { id: getId(), name: 'Laser Lair hart' },
  { id: getId(), name: 'Alfheim leicht' },
  { id: getId(), name: 'Alfheim hart' },
  { id: getId(), name: "Widow's Walkabout leicht" },
  { id: getId(), name: "Widow's Walkabout hart" },
  { id: getId(), name: 'Shangri-La leicht' },
  { id: getId(), name: 'Shangri-La hart' },
  { id: getId(), name: 'El Dorado leicht' },
  { id: getId(), name: 'El Dorado hart' },
  { id: getId(), name: 'Atlantis leicht' },
  { id: getId(), name: 'Atlantis hart' },
  { id: getId(), name: 'Temple at Zerzura leicht' },
  { id: getId(), name: 'Temple at Zerzura hart' },
  { id: getId(), name: '20,000 Leagues leicht' },
  { id: getId(), name: '20,000 Leagues hart' },
  { id: getId(), name: 'Journey leicht' },
  { id: getId(), name: 'Journey hart' },
];
// #endregion

// #region Events
export const events: Draft<MinigolfEvent>[] = [
  {
    id: getId(),
    date: new Date('2024-02-09'),
    registrationDeadline: new Date('2024-02-09T18:00:00Z'),
    timeslots: [
      {
        id: getId(),
        time: createTime(20, 0),
        mapId: maps[0].id,
        isFallbackAllowed: false,
        preconfigurations: [
          {
            id: getId(),
            playerIds: [users[0].id, users[1].id],
          },
        ],
        playerIds: [],
      },
      {
        id: getId(),
        time: createTime(21, 0),
        mapId: maps[1].id,
        isFallbackAllowed: true,
        preconfigurations: [],
        playerIds: [],
      },
      {
        id: getId(),
        time: createTime(22, 0),
        mapId: maps[2].id,
        isFallbackAllowed: false,
        preconfigurations: [],
        playerIds: [],
      },
    ],
  },
  {
    id: getId(),
    date: new Date('2024-01-26'),
    registrationDeadline: new Date('2024-01-26T18:00:00Z'),
    timeslots: [],
  },
  {
    id: getId(),
    date: new Date('2024-02-02'),
    registrationDeadline: new Date('2024-02-02T18:00:00Z'),
    timeslots: [],
  },
  {
    id: getId(),
    date: new Date('2024-01-19'),
    registrationDeadline: new Date('2024-01-19T18:00:00Z'),
    timeslots: [],
  },
];
// #endregion
