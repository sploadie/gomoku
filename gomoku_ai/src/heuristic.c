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

	return board->h + ((rand() % 8) - 3);
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
