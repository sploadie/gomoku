/* ************************************************************************** */
/*                                                                            */
/*                                                        :::      ::::::::   */
/*   read_board.c                                       :+:      :+:    :+:   */
/*                                                    +:+ +:+         +:+     */
/*   By: tgauvrit <tgauvrit@student.42.fr>          +#+  +:+       +#+        */
/*                                                +#+#+#+#+#+   +#+           */
/*   Created: 2016/12/17 12:26:44 by tgauvrit          #+#    #+#             */
/*   Updated: 2017/01/31 12:21:49 by tgauvrit         ###   ########.fr       */
/*                                                                            */
/* ************************************************************************** */

#include "gomoku.h"

t_board		*read_board(int fd, t_player *player)
{
	int		ret, len;
	char 	buf[BOARD_FILE_SIZE + 1];

	len = 0;
	while (len < BOARD_FILE_SIZE)
	{
		ret = read(fd, buf + len, BOARD_FILE_SIZE - len);
		if (ret == 0)
			continue ;
		if (ret < 1)
		{
			perror("Input");
			exit(1);
		}
		len += ret;
	}
	buf[BOARD_FILE_SIZE] = 0;
	return parse_board(buf, player);
}

t_board		*parse_board(char *string, t_player *player)
{
	int		i, j;
	t_board	board;

	if (strlen(string) != BOARD_FILE_SIZE)
	{
		perror("Input");
		exit(1);
	}
	for (i = 0; i < 15; ++i)
	{
		for (j = 0; j < 15; ++j)
		{
			board.map[i][j] = string[(j * 16) + i];
		}
	}
	*player = player_create(string[BOARD_FILE_SIZE - 2], 1);
	if (player->color == WHITE)
	{
		board.hrc[1].captured = string[BOARD_FILE_SIZE - 5] - '0';
		board.hrc[0].captured = string[BOARD_FILE_SIZE - 4] - '0';
	}
	else
	{
		board.hrc[0].captured = string[BOARD_FILE_SIZE - 5] - '0';
		board.hrc[1].captured = string[BOARD_FILE_SIZE - 4] - '0';
	}
	board_line5(&board, *player);
	board_free(&board, *player);
	heuristic_calculate(&board, player->maximizing);
	return memdup(&board, sizeof(t_board));
}
