# GCDC
A mod for Gamecraft-Discord chat connection using commands and text blocks.

## First-time setup
First invite the Discord bot to your server using [this link](https://discord.com/oauth2/authorize?client_id=680138144812892371&scope=bot) and make sure it has access to the channel you want to use. Then open the console in Gamecraft (`/` or the key near right shift by default) and do `dcsetup "channelid"` with the quotes. The channel ID can be obtained by enabling developer mode in Discord in User Settings -> Appearance and then right clicking the channel and clicking Copy ID.

When that is done, a browser window will open, asking you to give access to the Discord bot. This is only used to get basic info like your username to be displayed when you talk from the game and to avoid someone filling the database with junk. Also due to that, currently you can only have one channel linked at a time.

When you're logged in, the page will show a command to run in Gamecraft (it should be another `dcsetup` with a different argument). Then the initial setup is done.

## Usage
To send messages to Discord, use `dc "message"` with your message inside the quotes. To receive messages, place a text block named Discord. It will automatically update once messages are sent on Discord.
