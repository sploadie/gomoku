/* ************************************************************************** */
/*                                                                            */
/*                                                        :::      ::::::::   */
/*   print_board.c                                      :+:      :+:    :+:   */
/*                                                    +:+ +:+         +:+     */
/*   By: tgauvrit <tgauvrit@student.42.fr>          +#+  +:+       +#+        */
/*                                                +#+#+#+#+#+   +#+           */
/*   Created: 2016/12/17 13:08:22 by tgauvrit          #+#    #+#             */
/*   Updated: 2017/01/31 12:12:11 by tgauvrit         ###   ########.fr       */
/*                                                                            */
/* ************************************************************************** */

#include "gomoku.h"

void	print_board(t_board *board, t_player player)
{
	int 	i, j, c;
	char	buf[BOARD_FILE_SIZE];

	c = 0;
	for (j = 0; j < 15; ++j)
	{
		for (i = 0; i < 15; ++i)
			buf[c++] = board->map[i][j];
		buf[c++] = '\n';
	}
	if (player.color != WHITE)
		player = player_switch(player);
	buf[c++] = '0' + board->hrc[player.maximizing].captured;
	buf[c++] = '0' + board->hrc[player_switch(player).maximizing].captured;
	buf[c++] = '\n';
	buf[c++] = player_maximizing(player).color;
	buf[c] = '\n';
	write(1, buf, BOARD_FILE_SIZE);
	// More info
	dprintf(1, "White Free3: %d\n", board->hrc[player.maximizing].free3);
	dprintf(1, "White Free4: %d\n", board->hrc[player.maximizing].free4);
	dprintf(1, "White Line5: %d\n", board->hrc[player.maximizing].line5);
	player = player_switch(player);
	dprintf(1, "Black Free3: %d\n", board->hrc[player.maximizing].free3);
	dprintf(1, "Black Free4: %d\n", board->hrc[player.maximizing].free4);
	dprintf(1, "Black Line5: %d\n", board->hrc[player.maximizing].line5);
}