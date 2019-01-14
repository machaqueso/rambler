# Rambler
A multi-chat bot for streamers.

## Description:
Rambler is intended to be a single application consolidating the functions I require from several streamer assistance apps I currently use:

- streampro.io for banners
- nightbot for moderation and song requests in both twitch and youtube
- koalabot for twitch moderation, custom commands and points (chelas)
- restream.io multi-chat to read and show chat messages from both twitch and youtube

## Features:
When completed, Rambler 1.0 should have the following features:

- Support for Twitch and Youtube.
- A web server running on my local LAN I can use to read chat and perform moderation.
- A web page I can pull on OBS to show chat messages in stream.
  - Ignore list for users to never show in stream chat view (for bots)
- An automated reputation system:
  - Users gain reputation based on:
    - Length watching stream
    - Posting messages that do not break any moderation rules
  - Users lose reputation based on:
    - Posts breaking moderation rules (spam, repetition, links, words in black list)
  - Configurable reputation level for:
    - Messages to be shown on stream (so new accounts cannot post a bunch of swear words that will live forever in your stream's archive)
    - Messages ignored
    - User ignore/ban (from gaming site's chat)
  - Manually edit reputation (show upvote/downvote arrows next to user in message reader)
  - Reputation bypass Whitelist/Ignore/Blacklist
- Configurable point system (can name point as "chela" for example)
- Custom commands (to replace nightbot/koalabot)
- Special view widgets to pull into OBS showing follower alerts, view count, follow count, last follower.

## Wish list:
- Stand-alone and flexible web server architecture that can be deployed locally, hosted or as a SAAS like streampro or nightbot.
- Pluggable architecture to add support for gaming, moderation rules, etc.
- REST api

## Stream:
The plan is to stream while I'm developing this app, you can watch me code live at (twitch.tv/machacoder) and (https://www.youtube.com/channel/UCmUfkaiDlA3IZBibnMtyqlw)
