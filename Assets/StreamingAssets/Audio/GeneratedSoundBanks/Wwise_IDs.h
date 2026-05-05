/////////////////////////////////////////////////////////////////////////////////////////////////////
//
// Audiokinetic Wwise generated include file. Do not edit.
//
/////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef __WWISE_IDS_H__
#define __WWISE_IDS_H__

#include <AK/SoundEngine/Common/AkTypes.h>

namespace AK
{
    namespace EVENTS
    {
        static const AkUniqueID PLAY_CARD_ELEMENT = 4002306001U;
        static const AkUniqueID PLAY_CARD_MAIN = 2828349068U;
        static const AkUniqueID PLAY_CARD_WEAPON = 4293961391U;
        static const AkUniqueID PLAY_DEATH = 1172822028U;
        static const AkUniqueID PLAY_DMG = 3025804934U;
        static const AkUniqueID PLAY_FOOTSTEP = 1602358412U;
        static const AkUniqueID PLAY_HEAL = 2639148008U;
        static const AkUniqueID PLAY_SFX_UI_BACK_TO_TITLE_SCREEN = 3206396784U;
        static const AkUniqueID PLAY_SFX_UI_BUTTON_CLICK = 15941370U;
        static const AkUniqueID PLAY_SFX_UI_BUTTON_DROP_DOWN = 1765053760U;
        static const AkUniqueID PLAY_SFX_UI_BUTTON_HOVER = 4180039856U;
        static const AkUniqueID PLAY_SFX_UI_CREDITS_CLOSE = 3223191906U;
        static const AkUniqueID PLAY_SFX_UI_CREDITS_OPEN = 2400083638U;
        static const AkUniqueID PLAY_SFX_UI_OPTIONS_MENU_CLOSE = 3690523182U;
        static const AkUniqueID PLAY_SFX_UI_OPTIONS_MENU_OPEN = 1741661546U;
        static const AkUniqueID PLAY_SFX_UI_PAUSE_MENU_CLOSE = 451036422U;
        static const AkUniqueID PLAY_SFX_UI_PAUSE_MENU_OPEN = 3507907538U;
        static const AkUniqueID PLAY_SFX_UI_SLIDER = 1901608880U;
        static const AkUniqueID PLAY_SFX_UI_START_GAME = 1373152090U;
        static const AkUniqueID PLAY_TICK_DMG = 992468480U;
        static const AkUniqueID START_GAME = 1114964412U;
        static const AkUniqueID STOP_GAME = 210615102U;
    } // namespace EVENTS

    namespace STATES
    {
        namespace GAME_STATE
        {
            static const AkUniqueID GROUP = 766723505U;

            namespace STATE
            {
                static const AkUniqueID INGAME_STATE = 2678566748U;
                static const AkUniqueID INMENU_STATE = 3427763467U;
                static const AkUniqueID NONE = 748895195U;
            } // namespace STATE
        } // namespace GAME_STATE

        namespace LVL_STATE
        {
            static const AkUniqueID GROUP = 4066245315U;

            namespace STATE
            {
                static const AkUniqueID COMBAT = 2764240573U;
                static const AkUniqueID NONE = 748895195U;
                static const AkUniqueID RANDOM_COMBAT = 917864931U;
                static const AkUniqueID TITLE_SCREEN = 3853285476U;
                static const AkUniqueID TUTORIAL = 3762955427U;
            } // namespace STATE
        } // namespace LVL_STATE

        namespace MUSIC_STATE
        {
            static const AkUniqueID GROUP = 3826569560U;

            namespace STATE
            {
                static const AkUniqueID BOSSMUSIC_STATE = 1105946073U;
                static const AkUniqueID HIGHMUSIC_STATE = 118618844U;
                static const AkUniqueID LOWMUSIC_STATE = 395520184U;
                static const AkUniqueID MIDMUSIC_STATE = 3246761572U;
                static const AkUniqueID NONE = 748895195U;
            } // namespace STATE
        } // namespace MUSIC_STATE

        namespace PLAYER_STATE
        {
            static const AkUniqueID GROUP = 4071417932U;

            namespace STATE
            {
                static const AkUniqueID NONE = 748895195U;
                static const AkUniqueID PLAYERALIVE_STATE = 1031372527U;
                static const AkUniqueID PLAYERDEAD_STATE = 1265365118U;
            } // namespace STATE
        } // namespace PLAYER_STATE

        namespace ROOM_STATE
        {
            static const AkUniqueID GROUP = 2789492242U;

            namespace STATE
            {
                static const AkUniqueID INSIDE_STATE = 2599116615U;
                static const AkUniqueID NONE = 748895195U;
                static const AkUniqueID OUTSIDE_STATE = 1807907088U;
            } // namespace STATE
        } // namespace ROOM_STATE

    } // namespace STATES

    namespace SWITCHES
    {
        namespace ACTIONTYPE
        {
            static const AkUniqueID GROUP = 1026584191U;

            namespace SWITCH
            {
                static const AkUniqueID ATTACK = 180661997U;
                static const AkUniqueID BUFF = 1612179606U;
                static const AkUniqueID DEFENSE = 2564315215U;
                static const AkUniqueID HEAL = 3448274447U;
                static const AkUniqueID MAGIC = 1880439950U;
            } // namespace SWITCH
        } // namespace ACTIONTYPE

        namespace ELEMENTTYPE
        {
            static const AkUniqueID GROUP = 3055168915U;

            namespace SWITCH
            {
                static const AkUniqueID BLOOD = 3934470635U;
                static const AkUniqueID FIRE = 2678880713U;
                static const AkUniqueID NOELEMENT = 4009967300U;
                static const AkUniqueID POSION = 613188569U;
            } // namespace SWITCH
        } // namespace ELEMENTTYPE

        namespace POWERLVL
        {
            static const AkUniqueID GROUP = 2201078378U;

            namespace SWITCH
            {
                static const AkUniqueID HEAVY = 2732489590U;
                static const AkUniqueID LIGHT = 1935470627U;
            } // namespace SWITCH
        } // namespace POWERLVL

        namespace SCENETYPE
        {
            static const AkUniqueID GROUP = 1208296407U;

            namespace SWITCH
            {
                static const AkUniqueID COMBAT = 2764240573U;
                static const AkUniqueID RANDOMCOMBAT = 2460335360U;
                static const AkUniqueID TITLESCREEN = 152105657U;
                static const AkUniqueID TUTORIAL = 3762955427U;
            } // namespace SWITCH
        } // namespace SCENETYPE

        namespace SURFACETYPE
        {
            static const AkUniqueID GROUP = 63790334U;

            namespace SWITCH
            {
                static const AkUniqueID GENERAL = 133642231U;
                static const AkUniqueID GRAS = 906520814U;
                static const AkUniqueID STONE = 1216965916U;
                static const AkUniqueID WATER = 2654748154U;
                static const AkUniqueID WOOD = 2058049674U;
            } // namespace SWITCH
        } // namespace SURFACETYPE

        namespace SWITCHGRP_CHARTYPE
        {
            static const AkUniqueID GROUP = 2049048805U;

            namespace SWITCH
            {
                static const AkUniqueID ANIMAL = 767988009U;
                static const AkUniqueID HUMAN = 3887404748U;
                static const AkUniqueID ONI = 1064933107U;
            } // namespace SWITCH
        } // namespace SWITCHGRP_CHARTYPE

        namespace WEAPONTYPE
        {
            static const AkUniqueID GROUP = 767731869U;

            namespace SWITCH
            {
                static const AkUniqueID BOW = 546945295U;
                static const AkUniqueID CLAW = 3737052860U;
                static const AkUniqueID DAGGER = 3732162827U;
                static const AkUniqueID FIST = 2695658315U;
                static const AkUniqueID POTION = 4272075576U;
                static const AkUniqueID SPEAR = 573839388U;
            } // namespace SWITCH
        } // namespace WEAPONTYPE

    } // namespace SWITCHES

    namespace GAME_PARAMETERS
    {
        static const AkUniqueID RTPC_ATMO_VOL = 1766928022U;
        static const AkUniqueID RTPC_DIALOGUE_VOL = 3553633719U;
        static const AkUniqueID RTPC_MASTER_VOL = 460070935U;
        static const AkUniqueID RTPC_MENU_FOCUS = 2120441819U;
        static const AkUniqueID RTPC_MUSIC_VOL = 3061620274U;
        static const AkUniqueID RTPC_SFX_VOL = 3502123226U;
    } // namespace GAME_PARAMETERS

    namespace BANKS
    {
        static const AkUniqueID INIT = 1355168291U;
        static const AkUniqueID MAIN = 3161908922U;
    } // namespace BANKS

    namespace BUSSES
    {
        static const AkUniqueID BUS_ATMO = 1437783383U;
        static const AkUniqueID BUS_DIALOGUE = 2216040922U;
        static const AkUniqueID BUS_MUSIC = 1162281553U;
        static const AkUniqueID BUS_SFX = 3895923845U;
        static const AkUniqueID DX_2D_NARRATOR = 857912162U;
        static const AkUniqueID DX_3D_NPC = 1552314449U;
        static const AkUniqueID MAIN_AUDIO_BUS = 2246998526U;
        static const AkUniqueID MX_OVERWORLD = 3132888017U;
        static const AkUniqueID MX_TRAINGR = 2218867906U;
        static const AkUniqueID SFX_UI = 3862737079U;
    } // namespace BUSSES

    namespace AUX_BUSSES
    {
        static const AkUniqueID AUX_OPMENU_HALL = 865777238U;
    } // namespace AUX_BUSSES

    namespace AUDIO_DEVICES
    {
        static const AkUniqueID NO_OUTPUT = 2317455096U;
        static const AkUniqueID SYSTEM = 3859886410U;
    } // namespace AUDIO_DEVICES

}// namespace AK

#endif // __WWISE_IDS_H__
