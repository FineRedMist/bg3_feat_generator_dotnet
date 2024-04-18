# bg3_feat_generator_dotnet
A tool to generate the stat files with spells and boosts representing feats for the D&amp;D Epic 6 mod. 

For the D&amp;D Epic 6 implementation, I ran into a problem trying to dynamically create the spells and boosts within the Script Exttender.

Instead I wrote this to statically generate the boosts and spells that would represent the feats to grant in the [D&amp;D Epic 6 mod](https://github.com/FineRedMist/bg3_mod_epic6).

# Goals

The Feat Extractor goes through the pak files of the game, searching for all the data it would need to generate feats. It extracts module information, feats, feat descriptions, stats (including spells, passives, boosts, etc), lists (ability, skills, spells, etc) and tries to put that all together into E6_Gen_* files that you can see in the Epic 6 mod.

Long term it would have also downloaded mods from Nexus Mods to gather their feat information, too, however, given the impass described in the Epic 6 mod, I'm pausing development for now and making it available for others to consume and find uses for.

This code does use Norbyte's LSLib and has a batch file Get_LSLib_Deps.cmd, that will download it (the download folder is in the .gitignore).

# Why?

Why am I posting this if this solution doesn't work?
* I think even if something doesn't work, knowing that can save time when trying to find something that does.
* There may other useful tidbits in here others find useful and can extract.