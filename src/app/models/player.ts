export type Player = {
  id: string;
  alias: string | null;
  name: string;
  facebookId: string | null;
  whatsAppNumber: string | null;
  preferences: {
    avoid: string[];
    prefer: string[];
  };
};
