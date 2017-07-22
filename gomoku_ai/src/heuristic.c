/* ************************************************************************** */
/*                                                                            */
/*                                                        :::      ::::::::   */
/*   heuristic.c                                        :+:      :+:    :+:   */
/*                                                    +:+ +:+         +:+     */
/*   By: tgauvrit <tgauvrit@student.42.fr>          +#+  +:+       +#+        */
/*                                                +#+#+#+#+#+   +#+           */
/*   Created: 2016/12/18 10:53:28 by tgauvrit          #+#    #+#             */
/*   Updated: 2017/01/31 12:46:22 by tgauvrit         ###   ########.fr       */
/*                                                                            */
/* ************************************************************************** */

#include "gomoku.h"

// int	info[9];
// int[ 0:space_start, 1:space_end, 2:second_space, 3:free_three, 4:free_four, 5:five_win, 6:aligned, 7:total, 8:captured]
// x2 for maximizing(0) and minimizing(1) players
// #define SPACE_START  0
// #define SPACE_END    1
// #define SECOND_SPACE 2
// #define FREE_THREE   3
// #define FREE_FOUR    4
// #define FIVE_WIN     5
// #define ALIGNED      6
// #define TOTAL        7
// #define CAPTURED     8

// void		heuristic_calc(t_board *board)
// {
// 	// FIXME
// 	board->h = 0;
// }

// void		heuristic_calc_pos_line(t_board *board, int maximizing_color, t_move pos, int mod_x, int mod_y, int info[][])
// {
// 	int m, n;
// 	int minimizing_color;

// 	minimizing_color = (maximizing_color == WHITE ? BLACK : WHITE);
// 	for (m = -4; m < 1; ++m)
// 	{
// 		if (board_isvalid(pos.x + mod_x*(m-1), pos.y + mod_y*(m-1)) && board->map[pos.x + mod_x*(m-1)][pos.y + mod_y*(m-1)])
// 		{
// 			info[0][SPACE_START] = 1;
// 			info[1][SPACE_START] = 1;
// 		}
// 		if (board_isvalid(pos.x + mod_x*(m+5), pos.y + mod_y*(m+5)) && board->map[pos.x + mod_x*(m+5)][pos.y + mod_y*(m+5)])
// 		{
// 			info[0][SPACE_END] = 1;
// 			info[1][SPACE_END] = 1;
// 		}
// 		for (n = m; n < m + 5; ++n)
// 		{
// 			if (board_isvalid(pos.x + mod_x*n, pos.y + mod_y*n) == 0)
// 				continue ;
			
// 		}
// 	}
// }

// void		heuristic_calc_pos(t_board *board, t_player player, t_move pos, int info[][])
// {
// 	player = player_maximizing(player);

// 	if (player.color == WHITE) {
// 		info[0][CAPTURED] = board->white;
// 		info[1][CAPTURED] = board->black;
// 	} else {
// 		info[0][CAPTURED] = board->black;
// 		info[1][CAPTURED] = board->white;
// 	}
// }

// // Sets valid to 1 if valid move
// // Else sets valid to 0
// int			heuristic_calc_move(t_board board, t_player player, t_move move, int *valid)
// {
// 	// // FIXME
// 	// (void)player;
// 	// (void)move;
// 	// valid = 1;
// 	// board->h = 0;
// 	int	capture;
// 	int	info[2][2][8];

// 	board->map[move.x][move.y] = player.color;
// 	capture = board_capture(&board, move.x, move.y);
// }

int	heuristic_calculate(t_board *board, int is_maximizing)
{
	int			tmp;
	t_heuristic	*hrc = board->hrc;

	// captured
	if (hrc[1].captured > 9)
		return (board->h = INT_MAX);
	if (hrc[0].captured > 9)
		return (board->h = INT_MIN);
	board->h = (hrc[1].captured - hrc[0].captured) * 4;
	// line5
	if (hrc[1].line5 > 0)
	{
		if (is_maximizing == 1)
			board->h += 30;
		else
			return (board->h = INT_MAX);
	}
	if (hrc[0].line5 > 0)
	{
		if (is_maximizing == 0)
			board->h += -30;
		else
			return (board->h = INT_MIN);
	}
	// free4
	tmp = hrc[1].free4 - hrc[0].free4;
	tmp = tmp > 2 ? 2 : tmp;
	tmp = tmp < -2 ? -2 : tmp;
	board->h += tmp * 10;
	// free3
	tmp = hrc[1].free3 - hrc[0].free3;
	tmp = tmp > 3 ? 3 : tmp;
	tmp = tmp < -3 ? -3 : tmp;
	board->h += tmp * 4;

	return board->h;
}

t_board	*heuristic_partial_move(t_board *board, t_player player, int move_x, int move_y)
{
	int			prev_captured, prev_free3;

	// Save check values for double free3
	prev_captured = board->hrc[player.maximizing].captured;
	prev_free3 = board->hrc[player.maximizing].free3;
	// Handle placing chip, capture, free3, free4, and line5
	board_capture(board, player, move_x, move_y);
	// Handle double free3 (if needed)
	if (board->hrc[player.maximizing].captured == prev_captured)
		if (board->hrc[player.maximizing].free3 >= prev_free3 + 2)
			return (NULL);
	// Calculate partial heuristic
	// board->h = 0;
	heuristic_calculate(board, player.maximizing);
	return board;
}
