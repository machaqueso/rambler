# Rambler
A multi-chat bot for streamers.

## Description:
Rambler is an all-in-one, self-hosted tool for streamers.

## Features / Roadmap:
When completed, Rambler 1.0 should have the following features:

- Support for ~~Twitch~~, Discord, Mixer, etc.
- ~~A web server running on my local LAN I can use to read chat.~~ **DONE**
- ~~A web page I can pull on OBS to show chat messages in stream.~~ **DONE**
- ~~A web server running on my local LAN I can use to perform moderation.~~
  - ~~Ignore list for users to never show in stream chat view (for bots)~~
- An automated reputation system:
  - Users gain reputation based on:
    - Length watching stream
    - ~~Posting messages that do not break any moderation rules~~
  - Users lose reputation based on:
    - ~~Posts breaking moderation rules (spam, repetition, links, words in black list)~~
  - Configurable reputation level for:
    - Messages to be shown on stream (so new accounts cannot post a bunch of swear words that will live forever in your stream's archive)
    - Messages ignored
    - User ignore/ban (from gaming site's chat)
  - ~~Manually edit reputation (show upvote/downvote arrows next to user in message reader)~~
  - ~~Reputation bypass Whitelist/Ignore/Blacklist~~
- Configurable point system (can name point as "chela" for example)
- Custom commands (to replace nightbot/koalabot)
- Special view widgets to pull into OBS showing follower alerts, view count, follow count, last follower.

## Wish list:
- Stand-alone and flexible web server architecture that can be deployed locally, hosted or as a SAAS like streampro or nightbot.
- Pluggable architecture to add support for gaming, moderation rules, etc.
- REST api

## Stream:
The plan is to stream while I'm developing this app, you can watch me code live on [twitch](https://twitch.tv/machacoder) or [youtube](https://www.youtube.com/channel/UCmUfkaiDlA3IZBibnMtyqlw)

## FAQ:
- What happened to Youtube live chat support?

  Youtube has a [30 day retention policy for api data](https://developers.google.com/youtube/terms/developer-policies). 
  
  This policy makes it incompatible with some key features of our app such as reputation and the points system. (people who doesn't participate in chat for 30 days lose all their progress)
  
  While this app could still work within these restrictions, it will require reworking how the chat message flows are processed. 
  
  We decided to focus our efforts on integrating more friendly (and easier to work with) APIs from other providers such as Discord and circle back to youtube sometime in the future.     
