## ⚰️ SpritePackLoader has been discontinued ⚰️

The project's repository is archived as part of the GitHub Archive Program. SpritePackLoader's code and the documentation will no longer be updated. See more information in the [latest RogueLibs blog post](https://chasmical.github.io/RogueLibs/blog/2024/02/03/discontinuing-roguelibs). Feel free to fork the repository to continue working on the project.

# SpritePackLoader

<div align="center">
  <p>
    <a href="https://github.com/SugarBarrel/SpritePackLoader/releases/latest">
      <img src="https://img.shields.io/github/v/release/SugarBarrel/SpritePackLoader?label=Latest%20release&style=for-the-badge&logo=github" alt="Latest release"/>
    </a>
    <a href="https://github.com/SugarBarrel/SpritePackLoader/releases">
      <img src="https://img.shields.io/github/v/release/SugarBarrel/SpritePackLoader?include_prereleases&label=Latest%20pre-release&style=for-the-badge&logo=github" alt="Latest pre-release"/>
    </a>
    <br/>
    <a href="https://github.com/SugarBarrel/SpritePackLoader/releases">
      <img src="https://img.shields.io/github/downloads/SugarBarrel/SpritePackLoader/total?label=Downloads&style=for-the-badge" alt="Downloads"/>
    </a>
    <a href="https://github.com/SugarBarrel/SpritePackLoader/subscription">
      <img src="https://img.shields.io/github/watchers/SugarBarrel/SpritePackLoader?color=green&label=Watchers&style=for-the-badge" alt="Watchers"/>
    </a>
    <a href="https://github.com/SugarBarrel/SpritePackLoader/stargazers">
      <img src="https://img.shields.io/github/stars/SugarBarrel/SpritePackLoader?color=green&label=Stars&style=for-the-badge" alt="Stars"/>
    </a>
  </p>
</div>



## Installing SpritePackLoader

**[Follow the instructions on RogueLibs' site](https://sugarbarrel.github.io/RogueLibs/docs/user/installation)**.



## Installing SpritePacks

1. Install BepInEx, RogueLibs and SpritePackLoader *(obviously)*;
2. Download your .spritepack file that you want to use;
3. Put it in /BepInEx/spritepacks directory (create one if it doesn't exist);
4. Launch the game!



## Creating an Item SpritePack

1. Create a separate folder where you'll put all of your assets.
2. Create a subfolder called `ITEMS` inside of that folder.
3. Get your item sprite encoded in PNG or JPEG, and put it in `ITEMS`.
4. Make sure that the file's name matches the item's internal name. (e.g. `Blindenizer.png`)
5. Put the contents of your folder with assets into a .zip file.
6. Rename the .zip file to have a `.spritepack` extension.
7. Publish your spritepack!

The instructions for adding any other sprites are the same, just with a different subfolder:

- `ITEMS` - items and abilities;
- `OBJECTS` - objects;
- `FLOORS` - floor tiles;
- `BULLETS` - bullets and other projectiles;
- `HAIR` - hair;
- `FACIALHAIR` - facial hair;
- `HEADPIECES` - head pieces;
- `AGENTS` - eyes, heads, arms and legs;
- `BODIES` - bodies;
- `WRECKAGE` - wreckage, that spawns after destroying something;
- `INTERFACE` - interface stuff;
- `DECALS` - decals;
- `WALLTOPS` - tops of the walls;
- `WALLS` - sides of the walls;
- `SPAWNERS` - icons in level editor;
- `EXTRA` - extra sprites, don't worry about that;
- `AUDIO` - audio files (supported: .mp3, .ogg, .wav);

You can find the sprites' internal names in the tk2d archive below.

During development, you can simply put these subfolders into `<game root folder>/SpritePack` directory. Its contents will be read as if it were an archive. When you're finished you can just zip it and rename it to a .spritepack file.

## Resources

#### **[The tk2d archive, that contains all of the game's sprites](https://cdn.discordapp.com/attachments/433748059172896769/934322414932869140/tk2d.zip)**

#### **[The audio archive, that contains all of the game's audio](https://drive.google.com/file/d/1YnipH77glQcfON7LRrHPKpN-pku5sWua/view?usp=sharing)**

#### **[SpritePack template, with items and objects sprites](https://github.com/SugarBarrel/SpritePackLoader/releases/download/v1.0.1/SpritePackTemplate.zip)**



## Advanced SpritePack Creation

Don't want to create a subfolder for every single sprite type? That's okay! You can just add a `ITEMS_` prefix to the file's name! (e.g. `ITEMS_Blindenizer.png`)

Is your sprite too big? or perhaps too small? Try adjusting the PPU (pixels-per-unit) setting by suffixing the file's name with `_256x256`. (e.g. `Blindenizer_256x256.png`) The higher PPU is, the more detailed your sprite is, and the smaller it will appear in the game.

You can even create a subfolder called `ITEMS_256x256`. This way all sprites in the folder will have a higher PPU.

Also, feel free to create even more subfolders in the sprite-type folders. (e.g. `ITEMS/quest_items_8x8/Briefcase.png`) What matters is: the prefixes, suffixes and the name of the file.
